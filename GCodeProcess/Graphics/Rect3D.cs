using System.Diagnostics;

namespace GCodeProcess.Graphics;

public class Rect3D
{
    public Rect3D(Point3D abl, Point3D abr,Point3D atr,Point3D atl)
    {
        (bl, br, tr, tl) = (abl, abr, atr, atl);
    }

    public Point3D tl { get; set; }

    public Point3D tr { get; set; }

    public Point3D br { get; set; }

    public Point3D bl { get; set; }

    public bool ContainPoint(Point3D pd)
    {
        return bl.X <= pd.X && pd.X <= tr.X && bl.Y <= pd.Y && pd.Y <= tr.Y;
    }

    public (double a, double b, double c) GetSlope2D(Point3D p1, Point3D p2)
    {
        var v = p2 - p1;
        return (v.DY, -v.DX, -v.DY * p1.X + v.DX * p1.Y);
    }

    public bool inLine2D(Point3D p1, Point3D p2,Point3D p)
    {
        var d = p2 - p1;
        double t = -1;
        if (d.DX == 0 && d.DY == 0) return false;
        if (d.DX != 0)
            t = (p.X - p1.X) / d.DX;
        else
            t = (p.Y - p1.Y) / d.DY;
        return t >= 0 && t <= 1;
    }
    public bool GetIntersect(Point3D p1, Point3D p2,out Point3D p)
    {
        p = new Point3D(0, 0);
        var (a1, b1, c1) = GetSlope2D(p1, p2);

        var plist = new[] { bl, br, tr, tl, bl };
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
                if (inLine2D(p1, p2, p) && inLine2D(p3, p4, p)
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