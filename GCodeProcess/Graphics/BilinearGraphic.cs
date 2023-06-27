namespace GCodeProcess.Graphics;

public class BilinearGraphic
{
  
    public Point3D Interpolate(Point3D p1, Point3D p2, Point3D p3, Point3D p4, Point3D p0)
    {
        var lst = new List<Point3D>() { p1, p2, p3, p4 };
        var xSort = lst.OrderBy(o => o.X).ThenBy(o=>o.Y).ToList();
        var vx1 = new Vector3D(xSort[0], xSort[1]);
        var vx2 = new Vector3D(xSort[2], xSort[3]);
        var distVertical = new Vector2D(0, p0.Y - xSort[0].Y).Distance()/new Vector2D(0, xSort[1].Y - xSort[0].Y).Distance();
        
        
        var vxLeft = new Point3D(xSort[0].X,p0.Y, xSort[0].Z+vx1.Scale(distVertical).DZ);
        var vxRight = new Point3D(xSort[2].X,p0.Y, xSort[2].Z+vx2.Scale(distVertical).DZ);

        var vx3 = new Vector3D(vxLeft, vxRight);
        var distHorizontal = new Vector2D(p0.X-vxLeft.X, 0).Distance()/new Vector2D(vxRight.X-vxLeft.X, 0).Distance();
        var rs = new Point3D(p0)
        {
            Z = vxLeft.Z + vx3.Scale(distHorizontal).DZ
        };

        return rs;
    }
}