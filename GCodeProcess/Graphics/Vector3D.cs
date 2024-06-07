namespace GCodeProcess.Graphics;

public class Vector3D:Vector2D
{
  
   
    public double DZ { get; set; }
    public Vector3D(Point3D p1, Point3D p2) : base(p2.X - p1.X,p2.Y - p1.Y)
    {
      
      
        DZ = p2.Z - p1.Z;
    }
    // public static Point3D operator +(Vector3D p2,Point3D p1) => new Point3D(p1.X+p2.DX,p1.Y+p2.DY,p1.Z+p2.DZ);
    public static Vector3D operator +(Vector3D p2,Vector3D p1) => new Vector3D(p1.DX+p2.DX,p1.DY+p2.DY,p1.DZ+p2.DZ);

    public Vector3D(double dx, double dy, double dz=0) : base(dx,dy)
    {
      
        DZ = dz;
    }

    public override double Distance()
    {
        var sum = DX * DX + DY * DY+DZ * DZ;
       
        return Math.Sqrt(sum );
    }

    public Vector2D ToVector2D()
    {
        return new Vector2D(DX, DY);
    }

    public override Vector3D Normalize()
    {
        var d = Distance();
        return new Vector3D(DX / d, DY / d, DZ / d);
    }
    public override Vector3D Scale(double dist)
    {
        return new Vector3D(dist * DX, dist * DY,dist*DZ);
    }


    public Point3D ToPoint()
    {
        return new Point3D(DX, DY, DZ);
    }
}