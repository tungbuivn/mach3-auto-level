using System.Net;
using GCodeProcess.FlatCam;
using GCodeProcess.GCode;
using GCodeProcess.Graphics;

namespace GCodeProcess.HeightMap;

public class HeightMap : IRunableHandler
{
    private readonly HeightMapOptions _options;
    private readonly GCodeFactory _gCodeFactory;
    private HeightMapSetting _setting;

    public HeightMap(HeightMapOptions options, GCodeFactory gCodeFactory, Settings settings)
    {
        _options = options;
        _gCodeFactory = gCodeFactory;
        _options.MapFile = _options.MapFile!.ReplacePath();
        _setting = settings.HeightMap;
    }


    public List<Rect3D> Grid = new();

    public void Run()
    {
        var points = File.ReadAllLines(_options.MapFile!)
            .Select(o =>
            {
                var ar = o.Split(',');
                if (ar.Length > 2)
                    return new Point3D(ar[0].GetDouble(), ar[1].GetDouble(), ar[2].GetDouble());
                return null;
            })
            .Where(o => o != null)
            .OrderBy(o => o.Y)
            .ThenBy(o => o.X)
            .ToList();
        var min = new Point2D(points.Min(o => o.X), points.Min(o => o.Y));
        var max = new Point2D(points.Max(o => o.X), points.Max(o => o.Y));
        var v = new Vector2D(min, max);
        var numx = (int)(v.DX / 10);
        var numy = (int)(v.DY / 10);
        var ny1 = numy + 1;
        for (int i = 0; i < numy; i++)
        {
            for (int j = 0; j < numx; j++)
            {
                if (i * numy + j >= 0 && i * numy + j < points.Count)
                {
                    if ((i + 1) * numy + j + 1 >= 0 && (i + 1) * numy + j + 1 < points.Count)
                    {
                        var row = (i) * ny1;
                        var rowPlus1 = (i + 1) * ny1;
                        var rect = new Rect3D(points[row + j]!, points[row + j + 1]!, points[rowPlus1 + j + 1]!,
                            points[rowPlus1 + j]!);
                        // var rect = new Rect3D(points[i * ny1 + j]!, points[(i + 1) * ny1  + j + 1]!);
                        Grid.Add(rect);
                    }
                }
            }
        }

       
        // handling gcode file
        var curDir = _options.Directory.ReplacePath();
        foreach (var file in new[] { "cutout_nc.nc", "drill08.nc", "drill06.nc", "ncc_board.nc" })
        {
            
            var filePath = $"{curDir}/{file}";
            if (!File.Exists(filePath)) continue;
            var gc = _gCodeFactory.CreateNew();
            gc.ParseFile(filePath);
            var des = _gCodeFactory.CreateNew();
            for (int i = 0; i < gc.Data.Count; i++)
            {
                var shouldIgnore = gc.Data[i].IsComment;
                shouldIgnore = shouldIgnore || (gc.Data[i].Data[0].Op != 'G') || (gc.Data[i].Data[0].Val.GetInt() != 1);
                shouldIgnore = shouldIgnore ||
                               ((i - 1 < 0 || i - 1 >= gc.Data.Count || gc.Data[i - 1].IsComment ||
                                 gc.Data[i - 1].Data[0].Op != 'G') ||
                                (gc.Data[i - 1].Data[0].Val.GetInt() != 1));
                if (shouldIgnore)
                {
                    des.Data.Add(gc.Data[i].Copy());
                }
                else
                {
                    // current g1,prev g1
                    // try to break down
                    GCodeOp x1;
                    GCodeOp y1;
                    GCodeOp z1;

                    GCodeOp x2;
                    GCodeOp y2;
                    GCodeOp z2;

                    if (!gc.Data[i].TryGet('X', out x1)) x1 = gc.GetPrev('X', i);
                    if (!gc.Data[i].TryGet('Y', out y1)) y1 = gc.GetPrev('Y', i);
                    if (!gc.Data[i].TryGet('Z', out z1)) z1 = gc.GetPrev('Z', i);

                    if (!gc.Data[i - 1].TryGet('X', out x2)) x2 = gc.GetPrev('X', i - 1);
                    if (!gc.Data[i - 1].TryGet('Y', out y2)) y2 = gc.GetPrev('Y', i - 1);
                    if (!gc.Data[i - 1].TryGet('Z', out z2)) z2 = gc.GetPrev('Z', i - 1);
                    if (new[] { x1, y1, z1, x2, y2, z2 }.All(o => o != null)
                        && gc.Data[i - 1].Data[0].Op == 'G'
                        && gc.Data[i - 1].Data[0].Val.GetInt() == 1
                        && !((x1.Val == x2.Val && y1.Val == y2.Val))
                       )
                    {

                       
                        var rs = BuildPolyPoint(
                            new Point3D(x2.Val.GetDouble(), y2.Val.GetDouble(), z2.Val.GetDouble()),
                            new Point3D(x1.Val.GetDouble(), y1.Val.GetDouble(), z1.Val.GetDouble())
                        );
                       
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
            }

            for (int i = 0; i < des.Data.Count; i++)
            {
                if (!des.Data[i].IsComment)
                {
                    if ((des.Data[i].Data[0].Op == 'G') && des.Data[i].Data[0].Val.GetInt() == 1)
                    {
                        if (des.GetCoord(i, out var p) && p.Z<0)
                        {
                            des.Data[i].TrySet('Z',  SolveHeight(p).Z.Fmt());
                        }
                    }
                }
            }
            File.WriteAllText($"{curDir}/level_{file}",des.ToString());
            
        }
    }

    public List<Point3D> FindIntersect(Point3D p1, Point3D p2)
    {
        var rs = new List<Point3D>();
        Point3D p;
        foreach (var rec in Grid)
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
        v=v.Normalize();
        var rs = new List<Point3D>() { p1 };
        while (len < d)
        {
            var np = v.Scale(len) + p1;
            rs.Add(np);
            len += _setting.MaxSegmentLength;
        }
        rs.Add(p2);
        
        return rs;
    }

    private Point3D SolveHeight(Point3D p)
    {
        var gr = Grid.Where(r => r.ContainPoint(p)).FirstOrDefault();
        if (gr != null)
        {
            var bl = new BilinearGraphic();
            var ps = bl.Interpolate(gr.bl, gr.br, gr.tr, gr.tl, p);
            p.Z += ps.Z;
        }

        return p;
    }
}