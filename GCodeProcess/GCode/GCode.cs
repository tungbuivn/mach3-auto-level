using GCodeProcess.FlatCam;
using GCodeProcess.Graphics;

namespace GCodeProcess.GCode;

public class GCode
{
    public List<GCodeCommand> Data = new();

    public override string ToString() => string.Join("\n",
        Data.Select(o => o.IsComment? o.Text: 
            string.Join(" ", o.Data.Select(p => $"{p.Op}{p.Val}"))+ (string.IsNullOrEmpty(o.Comment)?"":"  ;")+o.Comment
            ));

    public bool IsCommand(string gc, int i)
    {
        var cmd = new GCodeCommand();
        cmd.Parse(gc.ToUpper());
        return IsCommand(cmd.Data[0], i);
    }
    public bool IsCommand(GCodeOp gc,int i)
    {
        var d = Data[i];
        if (d.IsComment) return false;
        if (!d.TryGet(gc.Op, out var v))
        {
            v = GetPrev(gc.Op, i);
        }

        return v != null && (v.Op == gc.Op) && (v.Val.GetInt() == gc.Val.GetInt());
    }

    public void Normalize()
    {
        // find first line contain 'Z', that is where to start
        var found = false;
        // Point3D minXYZ;
        MinXyz = new Point3D(0, 0, 0);
        // Point3D maxXYZ;
        MaxXyz = new Point3D(0, 0, 0);
        var firstX = true;
        var firstY = true;


        for (int i = 0; i < Data.Count; i++)
        {
            var cmd = Data[i];
            if (!cmd.IsComment)
            {
                GCodeOp tmp = null!;
                if (cmd.TryGet('X', out tmp))
                {
                    var fval = tmp.Val.GetDouble();
                    if (firstX)
                    {
                        MinXyz.X = fval;
                        MaxXyz.X = fval;
                        firstX = false;
                    }
                    else
                    {
                        MinXyz.X = Math.Min(MinXyz.X, fval);
                        MaxXyz.X = Math.Max(MaxXyz.X, fval);
                    }
                }

                if (cmd.TryGet('Y', out tmp))
                {
                    var fval = tmp.Val.GetDouble();
                    if (firstY)
                    {
                        MinXyz.Y = fval;
                        MaxXyz.Y = fval;
                        firstY = false;
                    }
                    else
                    {
                        MinXyz.Y = Math.Min(MinXyz.Y, fval);
                        MaxXyz.Y = Math.Max(MaxXyz.Y, fval);
                    }
                }

                if (found)
                {
                    if (!new[] { 'M', 'T' }.Contains(cmd.Data[0].Op))
                    {
                        TryAddData('G', i, 0);
                        // donot add xy, if it is circle then point is diffrent
                        // tryAddData('X', i, -1);
                        // tryAddData('Y', i, -1);
                        TryAddData('Z', i, -1);
                        // tryAddData('F', i, -1);
                        if (cmd.TryGet('Z', out tmp))
                        {
                            
                            var fval = tmp.Val.GetDouble();
                            if (fval < 0 && cmd.Data[0].Op == 'G' && cmd.Data[0].Val.GetInt() == 0)
                            {
                                cmd.Data[0].Val = "01";
                            }
                            MinXyz.Z = Math.Min(MinXyz.Z, fval);
                            MaxXyz.Z = Math.Max(MaxXyz.Z, fval);
                        }
                    }

                    cmd.UpdateText();
                }
                else
                {
                    if ((cmd.Data[0].Op == 'G') && (
                            new[] { 0, 1, 2, 3 }.Contains(cmd.Data[0].Val.GetInt())))
                    {
                        found = cmd.TryGet('Z', out tmp);
                        if (found)
                        {
                            var fval = tmp.Val.GetDouble();
                            MinXyz.Z = fval;
                            MaxXyz.Z = fval;
                        }
                    }
                }
            }
        }
    }

    private void TryAddData(char op, int i, int idx = -1)
    {
        var cmd = Data[i];
        if (cmd.Data.All(o => o.Op != op))
        {
            var prev = GetPrev(op, i);
            if (prev != null)
            {
                if (idx == -1)
                    cmd.Data.Add(prev);
                else
                    cmd.Data.Insert(idx, prev);
            }
        }
    }

    public GCodeOp GetPrev(char op, int i)
    {
        for (var j = i - 1; j >= 0; j--)
        {
            if (j < Data.Count)
            {
                var found = Data[j].Data.FirstOrDefault(o => o.Op == op);
                if (found != null)
                {
                    return new GCodeOp() { Op = found.Op, Val = found.Val };
                }
            }
        }

        return null!;
    }

    public Point3D MaxXyz { get; set; } = null!;

    public Point3D MinXyz { get; set; } = null!;

    public void ParseLines(List<string> lines)
    {
        Data = lines
            .Select(s => new GCodeCommand().Parse(s))
            .ToList();
        Normalize();

        // replace every G1 ZMax with G0
        foreach (var cmd in Data)
        {
            if (!cmd.IsComment && cmd.IsG0123() && cmd.TryGet('Z', out var tmp) && tmp.Val.GetDouble().IsEqual(MaxXyz.Z))
            {
                cmd.Data[0].Val = "0";
                cmd.UpdateText();
            }
        }
    }

    public bool GetCoord(int i,out Point3D p)
    {
        p = new Point3D();
        GCodeOp x1;
        GCodeOp y1;
        GCodeOp z1;
        if (!Data[i].TryGet('X', out x1)) x1 = GetPrev('X', i);
        if (!Data[i].TryGet('Y', out y1)) y1 = GetPrev('Y', i);
        if (!Data[i].TryGet('Z', out z1)) z1 = GetPrev('Z', i);
        if (x1 != null && y1 != null && z1 != null)
        {
            p = new Point3D(x1.Val.GetDouble(), y1.Val.GetDouble(), z1.Val.GetDouble());
            return true;
        }

        return false;
    }

    public void ParseFile(string file)
    {
        ParseLines(File.ReadAllLines(file).ToList());
    }
}