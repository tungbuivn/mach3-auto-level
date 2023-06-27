namespace GCodeProcess.GCode;

public class GCodeOp
{
    public char Op { get; set; }
    public string Val { get; set; } = null!;

    public GCodeOp(char op, string val)
    {
        Op = op;
        Val = val;
    }

    public GCodeOp()
    {
    }
    
}