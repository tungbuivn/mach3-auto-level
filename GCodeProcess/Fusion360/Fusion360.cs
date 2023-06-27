using GCodeProcess.FlatCam;
using GCodeProcess.GCode;

namespace GCodeProcess.Fusion360;

public class Fusion360 : IRunableHandler
{
    private readonly Fusion360Options _appOptions;
    private readonly GCodeFactory _gCodeFactory;

    public Fusion360(AppOptions appOptions, GCodeFactory gCodeFactory)
    {
        _appOptions = (Fusion360Options) appOptions;
        _gCodeFactory = gCodeFactory;
    }

    public void Run()
    {
        var gc = _gCodeFactory.CreateNew();
        var filename = _appOptions.FileName;
        if (filename != null)
        {
            if (!File.Exists(filename))
            {
                filename=System.IO.Directory.GetCurrentDirectory()+"/"+filename;
                
            }
            else
            {
                filename = Path.GetFullPath(filename);
            }
            Console.WriteLine("Process file:" +filename);
            gc.ParseFile(filename);
            File.WriteAllText(Path.GetDirectoryName(filename) + "/mod_" +
                              Path.GetFileName(filename), gc.ToString());
        }
    }
}