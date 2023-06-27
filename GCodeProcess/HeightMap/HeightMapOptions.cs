using CommandLine;

namespace GCodeProcess.FlatCam;

public class HeightMapOptions
{
    [Value(0, MetaName = "cmd", HelpText = "Gerber file process")]
    public string Cmd { get; set; } = "map";


    [Value(1, MetaName = "map-file", HelpText = "Mapfile to process.")]
    public string? MapFile { get; set; }

    [Value(2, MetaName = "directory", HelpText = "Mapfile to process.")]
    public string Para1 { get; set; }

    public string Directory
    {
        get
        {
            if (string.IsNullOrEmpty(Para1))
                return System.IO.Directory.GetCurrentDirectory();
            return Para1;
        }
       
    }
}