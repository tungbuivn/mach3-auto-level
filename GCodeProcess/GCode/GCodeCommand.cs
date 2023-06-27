using System.Text.RegularExpressions;
using GCodeProcess.FlatCam;

namespace GCodeProcess.GCode;

public class GCodeCommand
{
    public string Text { get; set; } = null!;

    public bool IsComment { get; set; }

    // public string Cmd { get; set; }
    public List<GCodeOp> Data { get; set; } = new();
    public string Comment { get; set; } = null!;

    public bool IsG0123()
    {
        if (IsComment) return false;
        if (TryGet('G', out var v))
        {
            return new[] { 0, 1, 2, 3 }.Contains(v.Val.GetInt());
        }

        return false;
    }

   

    public GCodeCommand Copy()
    {
        return new GCodeCommand()
        {
            Text = this.Text,
            Data = this.Data.ToArray().ToList(),
            IsComment = this.IsComment,
            Comment = Comment
        };
    }

    /// <summary>
    /// parse string into Data
    /// </summary>
    /// <param name="s"></param>
    public GCodeCommand Parse(string s)
    {
        s = (string.IsNullOrEmpty(s) ? "" : s).Trim();
        IsComment = string.IsNullOrEmpty(s) || s[0] == '(';
        if (!IsComment)
        {
            
            var m = Regex.Split(s,";");
            if (Regex.IsMatch(s, "\\("))
            {
                m = Regex.Split(s, "\\(");
                m[1] = "(" + m[1];
            }
            s = Regex.Replace(m[0], "([a-zA-Z])", " $1");
            if (m.Length > 1)
            {
                Comment = m[1];
            }
        }
            
        var arr = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        // var cmd=new GCodeCommand();
        Text = s;


        if (!IsComment)
        {
            // Cmd = arr[0];
            Data = arr.Select(s1 =>
                {
                    var op = new GCodeOp
                    {
                        Op = $"{s1[0]}".ToUpper()[0],
                        Val = s1[1..]
                    };
                    return op;
                }).ToLookup(o => o.Op)
                .Select(o => o.First())
                .ToList();
        }

        return this;
    }

    private static readonly GCodeComparer Comparer = new();

    /// <summary>
    /// Update text from data
    /// </summary>
    public void UpdateText()
    {
        if (!IsComment)
        {
            Text = string.Join(" ", Data.OrderBy(o => o.Op, Comparer).Select(o => $"{o.Op}{o.Val}"));
        }
    }

    public void TrySet(char op, string val)
    {
        GCodeOp gop;
        if (TryGet(op, out gop))
        {
            gop.Val = val;
           
            
        }
        else
        {
            Data.Add(new GCodeOp()
            {
                Op = op,
                Val = val
            });
        }
        UpdateText();
       
    }

    public bool TryGet(char op, out GCodeOp val)
    {
        val = Data.FirstOrDefault(o => o.Op == op)!;
        return val != null;
    }
}