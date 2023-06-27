using GCodeProcess.FlatCam;
using GCodeProcess.GCode;

namespace GCodeProcess.Fusion360;

public class Fusion360 : IRunableHandler
{
    private readonly Fusion360Options _appOptions;
    private readonly GCodeFactory _gCodeFactory;

    public Fusion360(Fusion360Options appOptions, GCodeFactory gCodeFactory)
    {
        _appOptions = appOptions;
        _gCodeFactory = gCodeFactory;
    }

    public void Run()
    {
        var gc = _gCodeFactory.CreateNew();
        if (_appOptions.FileName != null)
        {
            gc.ParseFile(_appOptions.FileName);
            File.WriteAllText(Path.GetDirectoryName(_appOptions.FileName) + "/mod_" +
                              Path.GetFileName(_appOptions.FileName), gc.ToString());
        }
    }
}