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
        (_svc.GetRequiredService(_appOptions.Handler) as IRunableHandler)?.Run();
    }
}