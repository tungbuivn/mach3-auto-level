using CommandLine;

namespace GCodeProcess.Fusion360;

public class Fusion360Options
{
    [Value(0, MetaName = "cmd", HelpText = "Process fusion 360 gcode file")]
    public string Cmd { get; set; } = "360";
    [Value(1, MetaName = "filename", HelpText = "File to process.")]
    public string? FileName { get; set; }
}