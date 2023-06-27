using CommandLine;

namespace GCodeProcess.HeightMap;

public class HeightMapOptions:AppOptions
{
    public override Type Handler => typeof(HeightMap);

    [Value(1, MetaName = "map-file", HelpText = "Mapfile to process.")]
    public string? MapFile { get; set; }

    [Value(2, MetaName = "directory", HelpText = "Mapfile to process.")]
    public string Para1 { get; set; } = null!;

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