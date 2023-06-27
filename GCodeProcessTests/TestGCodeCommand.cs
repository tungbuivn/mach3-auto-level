using GCodeProcess;
using GCodeProcess.FlatCam;
using GCodeProcess.GCode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GCodeProcessTests;

public class TestsGCodeCommand:BaseTest
{
   
  

    [Test]
    public void TestParse()
    {
        
        var a = new GCodeCommand();
        a.Parse("G1X5.6Y7.8Z-15.23");
        Assert.IsTrue(a.Data[0].Op== 'G');
        Assert.IsTrue(a.Data[1].Op=='X');
        Assert.IsTrue(a.Data[1].Val== "5.6");
    }
}