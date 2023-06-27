using GCodeProcess;
using GCodeProcess.FlatCam;
using GCodeProcess.GCode;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GCodeProcessTests;

public abstract class BaseTest
{
    protected IServiceProvider _serviceProvider = null!;
    [SetUp]
    public void Setup()
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(new string[]{});
        builder.RegisterIoc(new string[]{});
       
        _serviceProvider = builder.Build().Services;
    }
   
}