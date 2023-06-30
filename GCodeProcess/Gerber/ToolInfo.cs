namespace GCodeProcess.Gerber;

public class ToolInfo
{
    public bool HasMillingSlot { get; set; }
    // public int pos { get; set; }
    public double Size { get; set; }
    
    public string Name { get; set; } = null!;
    public string DrillGroup { get; set; } = null!;
    public string MillingGroup { get; set; } = null!;
}