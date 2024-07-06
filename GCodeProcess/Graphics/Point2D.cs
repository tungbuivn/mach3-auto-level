namespace GCodeProcess.Graphics;

public class Point2D
{
    public bool IsDeleted = false;
    public double X { get; set; }
    public double Y { get; set; }
    

    public Point2D()
    {
        
    }
    public Point2D(Point2D p)
    {
        X = p.X;
        Y = p.Y;
       
    }
    public Point2D(double x, double y)
    {
        X = x;
        Y = y;
      
    }
}