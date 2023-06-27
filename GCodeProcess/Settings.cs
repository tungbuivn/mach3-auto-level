using GCodeProcess.FlatCam;
using GCodeProcess.HeightMap;

namespace GCodeProcess;

public class Settings
{
    public FlatCamSettings FlatCam { get; set; } = null!;
    public HeightMapSetting HeightMap { get; set; } = null!;
}