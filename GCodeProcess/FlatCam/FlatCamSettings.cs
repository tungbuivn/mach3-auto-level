namespace GCodeProcess.FlatCam;

public class FlatCamSettings
{
    public string Ver85 { get; set; } = null!;
    public string Beta { get; set; } = null!;
    public double CutOutDiameter { get; set; }
    public float PcbDiameter { get; set; }
    public int SpindleSpeed { get; set; }
    public bool CleanPcb { get; set; }
    public double DrillDepth { get; set; }
    public double ZClearance { get; set; }
    public double ZFetchRate { get; set; }
    public double XyFetchRate { get; set; }
    public int ZProbeFetchRate { get; set; }
    public int XyProbeFetchRate { get; set; }
}