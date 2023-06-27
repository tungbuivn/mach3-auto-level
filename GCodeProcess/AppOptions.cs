using CommandLine;


public class AppOptions
{
    [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
    public bool Verbose { get; set; }

    [Value(0, MetaName = "cmd", HelpText = "command line process.ger,360")]
    public string Cmd { get; set; } = "ger";
    
    [Option('t', "top", Required = false, HelpText = "process top layer.")]
    public int Top { get; set; } = 0;
}