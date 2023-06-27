using CommandLine;

namespace GCodeProcess.FlatCam;

public class GerberOptions
{
    [Value(0, MetaName = "cmd", HelpText = "Gerber file process")]
    public string Cmd { get; set; } = "ger";
    
    [Option('t', "top", HelpText = "Process top layer ? 0 or 1. default is 0")]
    public int Top { get; set; } = 0;
    
    [Option('k', "kill", HelpText = "Kill Flatcam on done ? 0 or 1. default is 0")]
    public int Kill { get; set; } = 0;
    
    [Value(1, MetaName = "directory", HelpText = "Directory to process.")]
    public string? Para1 { get; set; }

    public string? Directory
    {
        get
        {
            if (string.IsNullOrEmpty(Para1))
            {
                return System.IO.Directory.GetCurrentDirectory();
            }

            return Para1;
        } 
        
    }
}