namespace GCodeProcess.Graphics;

public class Rect2D
{
    public Rect2D(Point2D ap1, Point2D ap2)
    {
        P1 = new Point2D(Math.Min(ap1.X, ap2.X), Math.Min(ap1.Y, ap2.Y));
        P3 = new Point2D(Math.Max(ap1.X, ap2.X), Math.Max(ap1.Y, ap2.Y));
        P2 = new Point2D(P3.X, P1.Y);
        P4 = new Point2D(P1.X, P3.Y);
    }

    protected Rect2D()
    {
       
    }

    public bool ContainPoint(Point2D pd)
    {
        return P1.X <= pd.X && pd.X <= P3.X && P1.Y <= pd.Y && pd.Y <= P3.Y;
    }

    public bool Intersect(Point2D pd1, Point2D pd2)
    {
        return false;
    }

    public Point2D P4 { get; set; } = null!;

    public Point2D P2 { get; set; } = null!;

    public Point2D P3 { get; set; } = null!;

    public Point2D P1 { get; set; } = null!;
}