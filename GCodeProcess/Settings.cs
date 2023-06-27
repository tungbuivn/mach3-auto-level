namespace GCodeProcess;

public class Settings
{
    public FlatCamSettings FlatCam { get; set; } = null!;
    public HeightMapSetting HeightMap { get; set; }
}

public class HeightMapSetting
{
    public double MaxSegmentLength { get; set; }
}