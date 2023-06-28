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

    public void Run()
    {
        var curDir = _options.Directory.ReplacePath();
        var fileName = _options.MapFile;
        if (!File.Exists(fileName))
        {
        }

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

        int countY = CountRow(_points!, true);
        TotalRow = countY;
        int countX = CountRow(_points!, false);
        TotalCol = countX;
        // GetByRc(0, 0);

        _min = new Point2D(_points.Min(o => o!.X), _points.Min(o => o!.Y));
        // new Point2D(_points.Max(o => o!.X), _points.Max(o => o!.Y));
        for (int i = 0; i < TotalCol; i++)
        {
            for (int j = 0; j < TotalRow - 1; j++)
            {
                if (GetByRc(i + 1, j + 1) != null)
                {
                    var rect = new Rect3D(GetByRc(i, j), GetByRc(i, j + 1), GetByRc(i + 1, j), GetByRc(i + 1, j + 1));
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

        var sources = new[] { "cutout_nc.nc", "drill08.nc", "drill06.nc", "ncc_board.nc", "top_layer.nc" };
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

            File.WriteAllText($"{curDir}/level_{file}", des.ToString());
            Console.WriteLine($@"Process file saved to: {curDir}/level_{file}");
        }
    }

    public int TotalCol { get; set; }

    public int TotalRow { get; set; }
    private readonly List<double> _rows = new ();
    private readonly List<double> _cols = new ();

    private Point3D GetByRc(int r, int c)
    {
        var idx = r * TotalRow + c;
        if (idx >= _points.Count) return null!;
        return _points[idx]!;
    }

    private int CountRow(List<Point3D> points, bool isY)
    {
        Point3D n = null!;
        int count = 1;
        foreach (var item in points.OrderBy(o => isY ? o.Y : o.X))
        {
            if (n == null)
            {
                n = item;
                if (!isY) _rows.Add(n.Y);
                else
                {
                    _cols.Add(n.X);
                }
            }
            else if ((isY && n.Y.IsEqual(item.Y, 0.1)) || (!isY && n.X.IsEqual(item.X, 0.1)))
            {
                count++;
                if (!isY) _rows.Add(item.Y);
                else
                {
                    _cols.Add(item.X);
                }
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
        int x=FindRange(p.Y,_rows);
        int y=FindRange(p.X,_cols);
        

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
        var row = (p.Y - _min.Y) / _height;
        var col = (p.X - _min.X) / _width;
        var (i, j) = ((int)Math.Floor(row.IsEqual(0)?0:row), (int)Math.Floor(col.IsEqual(0)?0:col));
        //if (i < 0 || j < 0) throw new Exception($"Khong the xac dinh vi tri diem tren grid ({p.X},{p.Y})");
        (i, j) = GetRowCol(p);
        // var gr = Grid.Where(r => r.ContainPoint(p)).FirstOrDefault();
        var gr = _grid[i * (TotalRow - 1) + j];
        
        if ((gr != null)&&(gr.Bl.X<=p.X)&&(gr.Bl.Y<=p.Y)&&(gr.Tr.X>=p.X)&&(gr.Tr.Y>=p.Y))
        {
            var bl = new BilinearGraphic();
            var ps = bl.Interpolate(gr.Bl, gr.Br, gr.Tr, gr.Tl, p);
            p.Z += ps.Z;
        }
        else throw new Exception("Point outside height map grid!");

        return p;
    }
}