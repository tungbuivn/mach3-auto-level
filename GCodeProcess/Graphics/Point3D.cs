using GCodeProcess.FlatCam;

namespace GCodeProcess.Graphics;

public class Point3D: Point2D
{
   
    public double Z { get; set; }

    public Point3D()
    {
        
    }
    public static Vector3D operator -(Point3D p1, Point3D p2) => new Vector3D(p2,p1);

    public bool IsEqual(Point3D p, bool ignoreZ = false,double precision =0.000001)
    {
        var v = p.X.IsEqual(X,precision) && p.Y.IsEqual(Y,precision);
        if (!ignoreZ)
        {
            v = v && p.Z.IsEqual(Z,precision);
        }

        return v;
    }
    public Point3D(Point3D p):base(p.X,p.Y)
    {
        
        Z = p.Z;
    }
    public Point3D(double x, double y, double z=0):base(x,y)
    {
        
        Z = z;
    }
}