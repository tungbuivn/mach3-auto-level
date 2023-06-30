using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using GCodeProcess.FlatCam;
using GCodeProcess.GCode;
using GCodeProcess.Graphics;

namespace GCodeProcess.HeightMap;

public class HeightMap : IRunableHandler
{
    private readonly HeightMapOptions _options;
    private readonly GCodeFactory _gCodeFactory;
    private HeightMapSetting _setting;

    public HeightMap(AppOptions options, GCodeFactory gCodeFactory, Settings settings)
    {
        _options = (HeightMapOptions)options;
        _gCodeFactory = gCodeFactory;
        _options.MapFile = _options.MapFile!.ReplacePath();
        _setting = settings.HeightMap;
    }

    private double _width;
    private double _height;

    private readonly List<Rect3D> _grid = new();
    private List<Point3D?> _points = null!;
    private Point2D _min = null!;

    void FixRpfMap(string mapFile)
    {
        var pa = Path.GetFullPath(mapFile);
        var dir = Path.GetDirectoryName(pa).ReplacePath();
        var filename = $"{dir}/rpf.nc";
        if (!File.Exists(filename)) return;
        Console.WriteLine($"Found rpc.nc ! Process repair \"{mapFile}\" file!");
        var lines = File.ReadAllLines(filename);
        var oldPoints = File.ReadAllLines(Path.GetFullPath(_options.MapFile!))
            .Select(o =>
            {
                var ar = Regex.Split(o, "[^0-9\\.\\+\\-]").Where(so => !string.IsNullOrEmpty(so)).ToArray();
                if (ar.Length > 2)
                    return new Point3D(ar[0].GetDouble(), ar[1].GetDouble(), ar[2].GetDouble());
                return null;
            }).ToList();
        var oldPos = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            if (Regex.IsMatch(lines[i], "^M40", RegexOptions.IgnoreCase))
            {
                var ls = new List<Point3D>();

                for (int j = i + 1; j < lines.Length; j++)
                {
                    if (Regex.IsMatch(lines[j], "^M41", RegexOptions.IgnoreCase))
                    {
                        if (oldPoints.Count != ls.Count)
                        {
                            Console.WriteLine("Number of point in rpf.nc not match ! ignore merge coordinates rpf.");
                        }
                        else
                        {
                            // check already replace
                            var done = Enumerable.Range(0, oldPoints.Count).All((k) => ls[k].IsEqual(oldPoints[k]));
                            if (done)
                            {
                                Console.WriteLine("File already repair !");
                                break;
                            }

                            // check coord match
                            var match = Enumerable.Range(0, oldPoints.Count)
                                .All((k) => ls[k].IsEqual(oldPoints[k], precision: 0.05));
                            if (!match)
                            {
                                Console.WriteLine("Coordinates does not match, ignore merge rpf points");
                            }
                            else
                            {
                                File.WriteAllLines(mapFile, ls.Select(p => $"{p.X},{p.Y},{p.Z}"));
                            }
                        }


                        // write file and exit
                        break;
                    }

                    if (Regex.IsMatch(lines[j], "^G1", RegexOptions.IgnoreCase))
                    {
                        var cmd = new GCodeCommand();
                        cmd.Parse(lines[j]);
                        cmd.TryGet('X', out var x);
                        cmd.TryGet('Y', out var y);
                        ls.Add(new Point3D(x.Val.GetDouble(), y.Val.GetDouble(), oldPoints[oldPos++].Z));
                    }
                }

                break;
            }
        }
    }

    public void Run()
    {
        var curDir = _options.Directory.ReplacePath();
        var fileName = _options.MapFile;
        if (!File.Exists(fileName))
        {
        }

        FixRpfMap(_options.MapFile);


        _points = File.ReadAllLines(_options.MapFile!)
            .Select(o =>
            {
                var ar = Regex.Split(o, "[^0-9\\.\\+\\-]").Where(so => !string.IsNullOrEmpty(so)).ToArray();
                if (ar.Length > 2)
                    return new Point3D(ar[0].GetDouble(), ar[1].GetDouble(), ar[2].GetDouble());
                return null;
            })
            .Where(o => o != null)
            .OrderBy(o => o!.Y)
            .ThenBy(o => o!.X)
            .ToList();

        int countY = CountRow(_points!);
        TotalRow = countY;
        _grid2D = new Point3D[_points.Count / TotalRow, TotalRow];
        var newsort = new List<Point3D>();
        int row = 0;
        while (_points.Any())
        {
            var ls = _points.Take(TotalRow).OrderBy(o => o.X).ToList();
            for (int i = 0; i < ls.Count; i++)
            {
                _grid2D[row, i] = ls[i];
            }

            row++;
            if (!_cols.Any())
            {
                _cols = ls.Select(o => o.X).ToList();
            }

            _points = _points.Skip(TotalRow).ToList();
            newsort.AddRange(ls);
            _rows.Add(ls[0].Y);
        }

        TotalCol = _rows.Count;
        // normalize coord
        var testX = new List<double>();
        var testY = new List<double>();
        for (int i = 0; i < TotalCol; i++)
        {
            for (int j = 0; j < TotalRow; j++)
            {
                _grid2D[i, j].X = _grid2D[0, j].X;
                _grid2D[i, j].Y = _grid2D[i, 0].Y;
                testX.Add(_grid2D[i, j].X);
                testY.Add(_grid2D[i, j].Y);
            }
        }

        if (testX.Distinct().Count() == TotalRow && testY.Distinct().Count() == TotalCol)
        {
        }
        else
        {
            throw new Exception("Invalid comlumn heightmap");
        }


        _points = newsort;


        // int countX = CountRow(_points!, false);
        // TotalCol = countX;
        _rows = _rows.OrderBy(o => o).ToList();
        _cols = _cols.OrderBy(o => o).ToList();
        // GetByRc(0, 0);

        _min = new Point2D(_points.Min(o => o!.X), _points.Min(o => o!.Y));
        // new Point2D(_points.Max(o => o!.X), _points.Max(o => o!.Y));
        for (int i = 0; i < TotalCol - 1; i++)
        {
            for (int j = 0; j < TotalRow - 1; j++)
            {
                {
                    var rect = new Rect3D(_grid2D[i, j], _grid2D[i, j + 1], _grid2D[i + 1, j + 1], _grid2D[i + 1, j]);
                    // var rect = new Rect3D(points[i * ny1 + j]!, points[(i + 1) * ny1  + j + 1]!);
                    _grid.Add(rect);
                }
            }
        }

        if ((_points.Count % TotalCol != 0) || (_points.Count % TotalRow != 0))
        {
            throw new Exception("So luong diem grid khong dung");
        }

        _width = _grid[0].Tr.X - _grid[0].Bl.X;
        _height = _grid[0].Tr.Y - _grid[0].Bl.Y;

        // handling gcode file

        var sources = new[]
        {
            "gb_cutout_nc.nc",
            "gb_bottom_layer.nc",
            "gb_drill06.nc",
            "gb_drill08.nc",
            "gb_milldrill08.nc",
            "gb_milled_slots08.nc",
            "gb_drill20.nc",
            "gb_milled_slots20.nc",
            "gb_top_layer.nc"
        };
        if (File.Exists(curDir))
        {
            curDir = Path.GetFullPath(curDir).ReplacePath();
            sources = new[] { Path.GetFileName(curDir) };
            curDir = Path.GetDirectoryName(curDir)!.ReplacePath();
        }


        foreach (var file in sources)
        {
            var filePath = $"{curDir}/level_{file}";
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        foreach (var file in sources)
        {
            var filePath = $"{curDir}/{file}";
            Console.WriteLine("Process file: " + filePath);
            if (!File.Exists(filePath)) continue;
            var gc = _gCodeFactory.CreateNew();
            gc.ParseFile(filePath);
            var des = _gCodeFactory.CreateNew();
            for (int i = 0; i < gc.Data.Count; i++)
            {
                if (i > 0 && gc.IsCommand("G1", i))
                {
                    gc.GetCoord(i - 1, out var p1);
                    gc.GetCoord(i, out var p2);

                    var rs = BuildPolyPoint(p1, p2);

                    foreach (var ln in rs.Skip(1).SkipLast(1))
                    {
                        var cm = new GCodeCommand();
                        cm.Parse($"G01 X{ln.X.Fmt()} Y{ln.Y.Fmt()} Z{ln.Z.Fmt()}");
                        cm.Comment = $"break line";
                        des.Data.Add(cm);
                    }

                    var mcp = gc.Data[i].Copy();

                    des.Data.Add(mcp);
                }
                else
                {
                    des.Data.Add(gc.Data[i].Copy());
                }
            }

            for (int i = 0; i < des.Data.Count; i++)
                if (des.IsCommand("G1", i) && des.GetCoord(i, out var p) && p.Z < 0)
                {
                    des.Data[i].TrySet('Z', SolveHeight(p).Z.Fmt());
                }

            var fnSave = $"{curDir}/alv_{file}";
            File.WriteAllText(fnSave, des.ToString());
            Console.WriteLine($@"Process file saved to: {fnSave}");
        }

        // process merger file after process done
        // is batch process ?
        if (sources.Length > 1)
        {
            MergeFile(new[] { "alv_gb_drill08.nc","alv_gb_milldrill08.nc", "alv_gb_milled_slots08.nc" }, "alv_gb_drillmill08.nc", curDir);
            MergeFile(new[] { "alv_gb_drill20.nc","alv_gb_milldrill08.nc", "alv_gb_milled_slots20.nc" }, "alv_gb_drillmill20.nc", curDir);
        }
    }

    private void MergeFile(string[] fs, string outname, string curDir)
    {
        var outfile = $"{curDir}/{outname}";
        if (File.Exists(outfile))
        {
            File.Delete(outfile);
        }

        var fl = fs.Select(o => $"{curDir}/{o}").Where(o => File.Exists(o)).ToList();
        var txt = string.Join("\n", fl.Select(o => File.ReadAllText(o)));
        // remove old file
        foreach (var s in fl)
        {
            Console.WriteLine("Process merging file: " + s);
            File.Delete(s);
        }

        // write new file
        if (!string.IsNullOrEmpty(txt)&&txt.Length>10)
            File.WriteAllText(outfile, txt);
    }

    public int TotalCol { get; set; }

    public int TotalRow { get; set; }
    private List<double> _rows = new();
    private List<double> _cols = new();
    private Point3D[,] _grid2D;

    private Point3D GetByRc(int r, int c)
    {
        var idx = r * TotalRow + c;
        if (idx >= _points.Count) return null!;
        return _points[idx]!;
    }

    private int CountRow(List<Point3D> points)
    {
        Point3D n = null!;
        int count = 1;
        foreach (var item in points)
        {
            if (n == null)
            {
                n = item;
            }
            else if ((n.Y.IsEqual(item.Y, 0.1)))
            {
                count++;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    public List<Point3D> FindIntersect(Point3D p1, Point3D p2)
    {
        var rs = new List<Point3D>();
        Point3D p;
        foreach (var rec in _grid)
        {
            if (rec.GetIntersect(p1, p2, out p))
            {
                goto next;
            }
        }

        rs.AddRange(new[] { p1, p2 });

        return rs;
        next:
        rs.AddRange(FindIntersect(p1, p));
        rs.AddRange(FindIntersect(p, p2));
        return rs;
    }

    private List<Point3D> BuildPolyPoint(Point3D p1, Point3D p2)
    {
        var v = p2 - p1;
        var d = v.Distance();
        var len = _setting.MaxSegmentLength;
        v = v.Normalize();
        var rs = new List<Point3D>() { p1 };
        // check if not vertical line
        if (!p1.X.IsEqual(p2.X) || !p1.Y.IsEqual(p2.Y))
            while (len < d && !(len.IsEqual(d)))
            {
                var np = v.Scale(len) + p1;
                rs.Add(np);
                len += _setting.MaxSegmentLength;
            }

        rs.Add(p2);

        return rs;
    }

    (int row, int col) GetRowCol(Point3D p)
    {
        int x = FindRange(p.Y, _rows);
        int y = FindRange(p.X, _cols);


        return (x, y);
    }

    private int FindRange(double val, List<double> data)
    {
        int rs = Int32.MaxValue;

        for (int i = 1; i < data.Count; i++)
        {
            if (data[i - 1] <= val && val <= data[i])
            {
                rs = i - 1;
                break;
            }
        }

        return rs;
    }

    private Point3D SolveHeight(Point3D p)
    {
        // var row = (p.Y - _min.Y) / _height;
        // var col = (p.X - _min.X) / _width;
        // var (k, l) = ((int)Math.Floor(row.IsEqual(0)?0:row), (int)Math.Floor(col.IsEqual(0)?0:col));
        //if (i < 0 || j < 0) throw new Exception($"Khong the xac dinh vi tri diem tren grid ({p.X},{p.Y})");
        var (i, j) = GetRowCol(p);
        // var p1 = _grid2D[i, j];
        // if (k !=i || l !=j)
        // {
        //     Debugger.Break();
        // }
        // var gr = _grid.Where(r => r.ContainPoint(p)).FirstOrDefault();
        var gr = _grid[i * (TotalRow - 1) + j];

        if (gr != null && gr.Bl.X <= p.X && gr.Bl.Y <= p.Y && gr.Tr.X >= p.X && gr.Tr.Y >= p.Y)
        {
            var bl = new BilinearGraphic();
            var ps = bl.Interpolate(gr.Bl, gr.Br, gr.Tr, gr.Tl, p);
            p.Z += ps.Z;
        }
        else throw new Exception("Point outside height map grid!");

        return p;
    }
}