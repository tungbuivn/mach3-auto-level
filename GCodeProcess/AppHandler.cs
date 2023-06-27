using Microsoft.Extensions.DependencyInjection;

namespace GCodeProcess;

public class AppHandler
{
    private readonly IServiceProvider _svc;
    private readonly AppOptions _appOptions;

    public AppHandler(IServiceProvider svc, AppOptions appOptions)
    {
        _svc = svc;
        _appOptions = appOptions;
    }
    public void Run()
    {
       
        Dictionary<string, Type> hdl = new()
        {
            {"ger",typeof(FlatCam.FlatCam)},
            {"360",typeof(Fusion360.Fusion360)},
            {"map",typeof(HeightMap.HeightMap)},
        };
        if (hdl.TryGetValue(_appOptions.Cmd, out var cls))
        {
            (_svc.GetRequiredService(cls) as IRunableHandler)?.Run();
        }
        
    }
}