namespace GCodeProcess.Graphics;

public class Rect2D
{
    public Rect2D(Point2D ap1, Point2D ap2)
    {
        this.p1 = new Point2D(Math.Min(ap1.X, ap2.X), Math.Min(ap1.Y, ap2.Y));
        this.p3 = new Point2D(Math.Max(ap1.X, ap2.X), Math.Max(ap1.Y, ap2.Y));
        this.p2 = new Point2D(p3.X, p1.Y);
        this.p4 = new Point2D(p1.X, p3.Y);
    }

    protected Rect2D()
    {
       
    }

    public bool ContainPoint(Point2D pd)
    {
        return p1.X <= pd.X && pd.X <= p3.X && p1.Y <= pd.Y && pd.Y <= p3.Y;
    }

    public bool Intersect(Point2D pd1, Point2D pd2)
    {
        return false;
    }

    public Point2D p4 { get; set; }

    public Point2D p2 { get; set; }

    public Point2D p3 { get; set; }

    public Point2D p1 { get; set; }
}