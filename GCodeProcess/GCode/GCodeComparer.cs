namespace GCodeProcess.GCode;

public class GCodeComparer : IComparer<char>
{
    private static Dictionary<char, int> cm = "GXYIJKZF".ToCharArray().Select((o, i) => (o, i))
        .ToDictionary(o => o.o, p => p.i);

    public int Compare(char x, char y)
    {
        var v1 = 0;
        var v2 = 0;
        if (cm.TryGetValue(x, out v1) && cm.TryGetValue(y, out v2))
        {
            if (v1 < v2) return -1;
            if (v1 > v2) return 1;
            return 0;
        }

        return x < y ? -1 : 1;
    }
}