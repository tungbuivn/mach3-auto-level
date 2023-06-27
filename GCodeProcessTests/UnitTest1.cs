using GCodeProcess;
using GCodeProcess.FlatCam;
using GCodeProcess.GCode;
using GCodeProcess.Graphics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GCodeProcessTests;

public class Tests:BaseTest
{
   
    [Test]
    public void TestGCode()
    {
        var fac=_serviceProvider.GetRequiredService<GCodeFactory>();
        var gc = fac.CreateNew();
        // gc.ParseFile("d:/0cnc/app/testcnc/test2.cnc");
        gc.ParseFile("d:/0cnc/app/__tests__/data/fusion-360.tap");
        File.WriteAllLines("test1.nc",gc.Data.Select(o=>o.Text));
        gc.ParseFile("c:/0pcb/wireless/ncc_board.nc");
    }
    [Test]
    public void TestGenerateRpf()
    {
        var fac=_serviceProvider.GetRequiredService<FlatCam>();
        fac.GenerateRpf("c:/0pcb/sdcard");
    }

    [Test]
    public void Test1()
    {
        
        var a = new BilinearGraphic();
        a.Interpolate(new Point3D(3, 3, 1), new Point3D(1, 1, 0), new Point3D(3, 1, 1), new Point3D(1, 3, 1),
            new Point3D(1.5, 1.5));
    }
}