using CommandLine;

namespace GCodeProcess.Fusion360;

public class Fusion360Options:AppOptions
{
    public override Type Handler => typeof(Fusion360);
    [Value(1, MetaName = "filename", HelpText = "File to process.")]
    public string? FileName { get; set; }
}