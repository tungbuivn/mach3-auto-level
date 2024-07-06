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

    string GetMillDrilling()
    {
        var toolDrillDia = _settingsFlatCam.DrillTools.Last();
        var toolsDias = string.Join(",", _tools.Where(o=>(int)(o.Size * 100)>(int)(toolDrillDia * 100)).Select(o => o.Size.ToString("0.000")));
        var key = ((int)toolDrillDia * 10).ToString().PadLeft(2, '0');
        if (!string.IsNullOrEmpty(toolsDias))
        {
            var milldrill = $@"milldrills drill -milled_dias {toolsDias} -tooldia {toolDrillDia} -outname geo_milldrill{key}
cncjob geo_milldrill{key} -dia {toolDrillDia} -z_cut {_settingsFlatCam.DrillDepth} -dpp 0.1 -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate {_settingsFlatCam.XyFetchRateMillDrill} -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname milldrill{key}_nc
write_gcode milldrill{key}_nc {Dir}/gb_milldrill{key}.nc
";
            return milldrill;
        }

        return "";
    }

    string GetDrillingByTool(List<ToolInfo> tl,string key)
    {

        var toolsDias = "";
        var availTools = _settingsFlatCam.DrillTools.Select((o, i) => new
        {
            Sise = o,
            Group = ((long)(o * 10)).ToString().PadLeft(2, '0'),
            index = i
        }).ToList();

        // for (var i = 0; i < _settingsFlatCam.DrillTools.Length;i++)
        // {
            var foundLst = availTools.Where(o => o.Group.Equals(key)).ToList();
            if (foundLst.Count > 0)
            {
                var found = foundLst[0];
                var qs = _tools.Where(o => (int)(o.Size*100) <= (int)(found.Sise*100));
                if (found.index > 0)
                {
                    qs = qs.Where(o => (int) (o.Size*100) > (int)(_settingsFlatCam.DrillTools[found.index - 1]*100));
                }
                toolsDias = string.Join(",", qs.Select(o=>o.Size.ToString("0.000")));
                // break;
            }
            else
            {
                // we not drill hole with greater last tool, milldrill will process it.
            }
           
        // }
        

        // if (key == "06")
        // {
        //     toolsDias = string.Join(",", tl.Where(o => (o.Size<=0.6)).Select(o=>o.Size));
        // } else  if (key == "08")
        // {
        //     toolsDias = string.Join(",", tl.Where(o => (o.Size>0.6) && (o.Size<=0.8)).Select(o=>o.Size));
        // } else  if (key == "10")
        // {
        //     toolsDias = string.Join(",", tl.Where(o => (o.Size>0.8) && (o.Size<=1.0)).Select(o=>o.Size));
        // } else 
        //     toolsDias = string.Join(",", tl.Where(o => (o.Size>1.0)).Select(o=>o.Size));
       
        if (string.IsNullOrEmpty(toolsDias)) return "";
        // process drill
        // var lst=tl.Select(o =>
        // {
            return $@"
drillcncjob drill -drilled_dias {toolsDias} -drillz {_settingsFlatCam.DrillDepth} -travelz {_settingsFlatCam.ZClearance} -feedrate_z {_settingsFlatCam.ZFetchRate} -spindlespeed {_settingsFlatCam.SpindleSpeed} -pp default -outname drill{key} 
write_gcode drill{key} {Dir}/gb_drill{key}.nc
";
            
        // }).ToList();
        //
        //
        // return string.Join("\n",lst);
    }

    string GetMillingByTool(List<ToolInfo> tl, string key)
    {
        // ignore all milling with diameter less than 0.8mm
        if (key == "06") return "";
        if (key == "08") return "";
        var toolMill = (key == "10" ? 1.0 : 2.0);
        var toolsDias = string.Join(",", tl.Where(o=>o.HasMillingSlot).Select(o => o.Size).Distinct());
        var dpp = toolMill<2?0.1:0.2;
        if (string.IsNullOrEmpty(toolsDias)) return "";
        
        // var lst = tl.Select(o =>
        // {
            return $@"
millslots drill -milled_dias ""{toolsDias}"" -tooldia {toolMill} -diatol 0 -outname milled_slots{key}
cncjob milled_slots{key} -dia {toolMill} -z_cut {_settingsFlatCam.DrillDepth} -dpp {dpp} -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate {_settingsFlatCam.XyFetchRateMillDrill} -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname milled_slots{key}_nc
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
        var millDrill=GetMillDrilling();
        var drill=string.Join("\n", _settingsFlatCam.DrillTools.Select(t =>
        {
            var group = $"{(int)(t * 10)}".PadLeft(2,'0');
            return GetDrillingByTool(new List<ToolInfo>(), group);
        }));
        // var drill = string.Join("\n", 
        //     _tools.GroupBy(o => o.DrillGroup).Select(g =>
        // {
        //     return GetDrillingByTool(g.ToList(), g.Key);
        // }));
        var mill = string.Join("\n", 
            _tools.GroupBy(o => o.MillingGroup)
                .Where(g=>!string.IsNullOrEmpty(g.Key))
                // .Where(g=>!(new[]{"06","08"}.Contains(g.Key)))
                .Select(g =>
            {
                return GetMillingByTool(g.ToList(), g.Key);
            }));
       

        return $"{millDrill}\n{drill}\n{mill}";
    }

    // load drill file
    public void LoadFile(string fileName)
    {
        
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
                string drillGroup = "";
                string millingGroup="";
                var foundList=this._settingsFlatCam.DrillTools
                    .Select((o,iv)=>(o,i:iv))
                    .Where(o => size <= o.o)
                    .ToList();
               
                if (foundList.Count > 0)
                {
                    if (size <= foundList[0].o)
                    {
                        drillGroup =  ((long)(size*10)).ToString().PadLeft(2,'0');
                        if (foundList[0].i > 0)
                        {
                            // already process by preview tool ?
                            if (size <= _settingsFlatCam.DrillTools[foundList[0].i - 1])
                            {
                                drillGroup = "";
                            }
                        }
                    }
                }
                else
                {
                    drillGroup = "";
                    millingGroup= ((long)(size*10)).ToString().PadLeft(2,'0');
                }
               
                // if (size <= 0.6) drillGroup = "06";
                // else  if (size <=0.8) drillGroup = "08"; 
                // else  if (size <=1.0) drillGroup = "10"; 
                // else
                // {
                //     drillGroup = "";
                // }

                // if milling size <=2mm then using drill 8mm, note, it will not milling hole less than 8mm
                
                // if (size < 0.8)
                // {
                //     // ignore this
                //     millingGroup = "06";
                // }else
                // if (size <= 1) millingGroup = ""; else
                // if (size < 2) millingGroup = "10";
                // else
                // {
                //     millingGroup = "20";
                // }
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