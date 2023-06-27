using System.Text.RegularExpressions;

namespace GCodeProcess.FlatCam;

public static class UtilExtension
{
    public static bool IsEqual(this double a, double b, double tolerance = 0.001f)
    {
        return Math.Abs(a - b) < tolerance;
    }

    public static string RemoveLeadZero(this string a)
    {
        // var rm=a.TrimStart('0');
        // if (string.IsNullOrEmpty(rm)) rm = "0";
        return a;
    }

    public static string Fmt(this double val) => $"{val:0.0000}";
    public static string ReplacePath(this string str) => Regex.Replace(str, @"[\\/]", "/").TrimEnd('/');

    public static double GetDouble(this string a)
    {
        return double.Parse(a.RemoveLeadZero());
    }

    public static long GetLong(this string a)
    {
        return long.Parse(a.RemoveLeadZero());
    }

    public static int GetInt(this string a)
    {
        return int.Parse(a.RemoveLeadZero());
    }

    public static bool IsGreater(this double a, double b)
    {
        return a > b;
    }

    public static bool IsLess(this double a, double b)
    {
        return a < b;
    }
}