using System.Diagnostics;

namespace GCodeProcess.Graphics;

public class Rect3D
{
    private Point2D _max;
    private Point2D _min;

    public Rect3D(Point3D abl, Point3D abr,Point3D atr,Point3D atl)
    {
        var st = new[] { atr, atl, abl, abr }.OrderBy(o => o.X).ThenBy(o => o.Y).ToArray();
        (Bl, Br, Tr, Tl) = (st[0],st[2],st[3],st[1]);
        _min = new Point2D(st.Min(o => o.X), st.Min(o => o.Y));
        _max = new Point2D(st.Max(o => o.X), st.Max(o => o.Y));
        //(bl, br, tr, tl) = (abl, abr, atr, atl);
    }

    public Point3D Tl { get; set; }

    public Point3D Tr { get; set; }

    public Point3D Br { get; set; }

    public Point3D Bl { get; set; }

    public bool ContainPoint(Point3D pd)
    {
        return _min.X <= pd.X && pd.X <= _max.X && _min.Y <= pd.Y && pd.Y <= _max.Y;
    }

    public (double a, double b, double c) GetSlope2D(Point3D p1, Point3D p2)
    {
        var v = p2 - p1;
        return (v.DY, -v.DX, -v.DY * p1.X + v.DX * p1.Y);
    }

    public bool InLine2D(Point3D p1, Point3D p2,Point3D p)
    {
        var d = p2 - p1;
        bool inLine2D=false;
        if (d.DX != 0 || d.DY != 0)
        {
            double t = -1;
            if (d.DX != 0)
                t = (p.X - p1.X) / d.DX;
            else
                t = (p.Y - p1.Y) / d.DY;
            inLine2D = t >= 0 && t <= 1;
            // return inLine2D;
        }

        return inLine2D;
    }
    public bool GetIntersect(Point3D p1, Point3D p2,out Point3D p)
    {
        p = new Point3D(0, 0);
        var (a1, b1, c1) = GetSlope2D(p1, p2);

        var plist = new[] { Bl, Br, Tr, Tl, Bl };
        for (int i = 0; i < plist.Length-1; i++)
        {
            var (p3, p4) = (plist[i], plist[i + 1]);
            var (a2, b2, c2) = GetSlope2D(p3,p4);
            var d = (a1 * b2 - a2 * b1);
            if (d != 0)
            {
                var x = (b1 * c2 - b2 * c1) / d;
                var y = (a2 * c1 - a1 * c2) / d;
                p = new Point3D(x, y);
                if (InLine2D(p1, p2, p) && InLine2D(p3, p4, p)
                                        && !p.IsEqual(p1,true) && !p.IsEqual(p2,true)
                                        && !p.IsEqual(p3,true) && !p.IsEqual(p4,true)
                   )
                {
                    return true;
                }
            }
           
        }
        return false;
    }
}