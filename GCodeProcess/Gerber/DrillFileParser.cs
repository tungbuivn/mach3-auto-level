using System.Text.RegularExpressions;
using GCodeProcess.FlatCam;

namespace GCodeProcess.Gerber;

public class DrillFileParser
{
    private readonly List<ToolInfo> _tools = new();
    // private string[] _allLines = null!;
    private readonly List<string> _files = new();
    private readonly FlatCamSettings _settingsFlatCam;

    public DrillFileParser(Settings settings)
    {
        _settingsFlatCam= settings.FlatCam;
    }

    string GetDrillingByTool(List<ToolInfo> tl,string key)
    {

        var toolsDias = string.Join(",", tl.Select(o => o.Size));
        if (string.IsNullOrEmpty(toolsDias)) return "";
        // process drill
        // var lst=tl.Select(o =>
        // {
            return $@"
drillcncjob drill -drilled_dias {toolsDias} -drillz {_settingsFlatCam.DrillDepth} -travelz {_settingsFlatCam.ZClearance} -feedrate_z {_settingsFlatCam.ZFetchRate} -spindlespeed {_settingsFlatCam.SpindleSpeed} -pp default -outname drill{key} 
write_gcode drill{key} {Dir}/gb_drill{key}.nc";
            
        // }).ToList();
        //
        //
        // return string.Join("\n",lst);
    }

    string GetMillingByTool(List<ToolInfo> tl, string key)
    {
        // ignore all milling with diameter less than 0.8mm
        if (key == "06") return "";
        var toolMill = (key == "08" ? 0.8 : 2.0);
        var toolsDias = string.Join(",", tl.Where(o=>o.HasMillingSlot).Select(o => o.Size).Distinct());
        var dpp = toolMill<2?0.1:0.2;
        if (string.IsNullOrEmpty(toolsDias)) return "";
        
        // var lst = tl.Select(o =>
        // {
            return $@"
millslots drill -milled_dias ""{toolsDias}"" -tooldia {toolMill} -diatol 0 -outname milled_slots{key}
cncjob milled_slots{key} -dia {_settingsFlatCam.PcbDiameter} -z_cut -2 -dpp {dpp} -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate 50 -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname milled_slots{key}_nc
write_gcode milled_slots{key}_nc {Dir}/gb_milled_slots{key}.nc
";
        // });
        // return String.Join("\n",lst);
    }

    public string GetMergeFileScript()
    {
        string drillStr;
        if (_files.Count == 1)
        {
            drillStr = $"open_excellon {_files[0]} -outname drill";
        }
        else
        {
            var joinFiles = new List<string>();
            var data =  _files.Select((f, i) =>
            {
                var code = $"drill{i}";
                joinFiles.Add(code);
                return $"open_excellon {f} -outname {code}";


            }).ToList();
            var mergeFileDrl = string.Join("\n", data);
            var joinStr="join_excellons drill " +
                     String.Join(" ", joinFiles);
            drillStr = $"{mergeFileDrl}\n{joinStr}";
        }
       
        
        return drillStr;
    }

    public string GetScriptDrillMill()
    {
        var drill = string.Join("\n", 
            _tools.GroupBy(o => o.DrillGroup).Select(g =>
        {
            return GetDrillingByTool(g.ToList(), g.Key);
        }));
        var mill = string.Join("\n", 
            _tools.GroupBy(o => o.MillingGroup).Select(g =>
            {
                return GetMillingByTool(g.ToList(), g.Key);
            }));
       

        return $"{drill}\n{mill}";
    }
    

    public void ParseFile(string fileName)
    {
        var allLines = File.ReadAllLines(fileName);
        Dir = Path.GetDirectoryName(Path.GetFullPath(fileName))!.ReplacePath();
        int i;
        var toolList = new List<ToolInfo>();
        for (i = 0; i < allLines.Count(); i++)
        {
            var str = allLines[i];
            if (str.Trim().Equals("%")) break;
            var m = Regex.Match(str, @"^(T\d+)C(\d+\.?\d+)");
            if (m.Success)
            {
                var toolName = m.Groups[1].Value;
                var size = double.Parse(m.Groups[2].Value);
                string drillGroup;
                if (size < 0.8) drillGroup = "06";
                else
                {
                    drillGroup = "08";
                }

                // if milling size <=2mm then using drill 8mm, note, it will not milling hole less than 8mm
                string millingGroup;
                if (size < 0.8)
                {
                    // ignore this
                    millingGroup = "06";
                }else
                if (size <= 2) millingGroup = "08";
                else
                {
                    millingGroup = "20";
                }
                toolList.Add(new ToolInfo()
                {
                    Name=toolName,
                    DrillGroup=drillGroup,
                    MillingGroup=millingGroup,
                    // pos = i,
                    Size = size
                });
                
            }
        }

        var hasData = false;

        for (var j = i; j < allLines.Length; j++)
        {
            if (Regex.IsMatch(allLines[j], "G85", RegexOptions.IgnoreCase))
            {
                var t = GetTool(allLines,toolList,j - 1);
                if ( t != null && !t.HasMillingSlot)
                {
                    t.HasMillingSlot = true;
                }
               
            }

            hasData = hasData || allLines[j][0] == 'X';
        }
        if (hasData)
        {
            foreach (var t in toolList)
            {
                var nt = _tools.FirstOrDefault(o => o.Size.IsEqual(t.Size));
                if (nt != null)
                {
                    nt.HasMillingSlot = nt.HasMillingSlot || t.HasMillingSlot;
                }
                else
                {
                    _tools.Add(t);
                }
            }
            
            _files.Add(fileName);
        }
    }

    private string Dir { get; set; } = null!;

    ToolInfo GetTool(string[] allLines, List<ToolInfo> toolInfos, int pos)
    {
        if (pos < 0) return null!;
        
        if (Regex.IsMatch(allLines[pos],"^T\\d+",RegexOptions.IgnoreCase))
        {
            return toolInfos.FirstOrDefault(o => o.Name.Equals(allLines[pos]))!;
        }

        return GetTool(allLines,toolInfos,pos - 1);
    }
    
}