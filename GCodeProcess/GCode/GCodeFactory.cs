namespace GCodeProcess.GCode;

public class GCodeFactory
{
    private readonly IServiceProvider _serviceProvider;

    public GCodeFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public GCodeProcess.GCode.GCode CreateNew()
    {
        return (_serviceProvider.GetService(typeof(GCodeProcess.GCode.GCode)) as GCodeProcess.GCode.GCode)!;
    }
   
}