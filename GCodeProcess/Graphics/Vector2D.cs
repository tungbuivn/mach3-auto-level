namespace GCodeProcess.Graphics;

public class Vector2D
{
    public double DX { get; protected set; }
    public double DY { get; protected set; }
    public Vector2D(Point2D p1, Point2D p2)
    {
        DX = p2.X-p1.X;
        DY = p2.Y-p1.Y;
    }
    public Vector2D(double dx, double dy)
    {
        DX = dx;
        DY = dy;
    }
    public virtual double Distance()
    {
        var sum = DX * DX + DY * DY;
     
        return Math.Sqrt(sum );
    }

    public virtual Vector2D Normalize()
    {
        var d = Distance();
        return new Vector2D(DX / d, DY / d);
    }

    public virtual Vector2D Scale(double dist)
    {
        return new Vector2D(dist * DX, dist * DY);
    }
}