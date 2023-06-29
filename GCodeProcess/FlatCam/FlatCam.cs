using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;
using GCodeProcess.GCode;
using GCodeProcess.Graphics;

namespace GCodeProcess.FlatCam;

public class FlatCam:IRunableHandler
{
    private const double MillingDia1 = 1.21;
    private const double MillingDia2 = 2;
    private readonly GerberOptions _appOptions;
    private readonly GCodeFactory _gCodeFactory;


    // only contain valid tool from drill file
    private readonly List<double> _drillTools = new List<double>();
    private readonly List<String> _drillFilesValid = new();
    private readonly FlatCamSettings _settingsFlatCam;

    public FlatCam(Settings settings, AppOptions appOptions, GCodeFactory gCodeFactory)
    {
        _appOptions = (GerberOptions) appOptions;
        _gCodeFactory = gCodeFactory;


        _settingsFlatCam = settings.FlatCam;
    }

   
    // replace all tool drl file

    public void Run()
    {
        var dir = _appOptions.Directory!.ReplacePath();
        var files = Directory.GetFiles(_appOptions.Directory ?? ".").Select(s =>
        {
            return new FileTypeAct
            {
                FileName = s.ReplacePath(),
                Ext = Path.GetExtension(s).ToLower(),
            };
        }).GroupBy(o => o.Ext).ToList();
        // replace knife size
        var drillFiles = files.Where(o => o.Key.Equals(".drl")).SelectMany(o => o).ToList();
        foreach (var fileTypeAct in drillFiles)
        {
            ProcessTool(fileTypeAct);
        }

        // var drillNcFiles = new List<string>();
        var drillGCodeExport = String.Join("\n", _drillTools.Select(t => (int)(t * 10)).Distinct().Select(tl =>
        {
            var file = (tl + "").PadLeft(2, '0');
            // drillNcFiles.Add($"{dir}/drill{file}.nc");
            var ret = $@"
drillcncjob drill -drilled_dias {tl / 10.0} -drillz {_settingsFlatCam.DrillDepth} -travelz {_settingsFlatCam.ZClearance} -feedrate_z {_settingsFlatCam.ZFetchRate} -spindlespeed {_settingsFlatCam.SpindleSpeed} -pp default -outname drill{file} 
write_gcode drill{file} {dir}/drill{file}.nc";
            return ret;
        }));
        var drillStr = "";

        if (_drillFilesValid.Any())
        {
            drillStr = $"open_excellon {_drillFilesValid[0]} -outname drill";

            if (_drillFilesValid.Count() > 1)
            {
                drillStr = String.Join("\n",
                    _drillFilesValid.Select((o, i) => { return $"open_excellon {o} -outname drill{i}"; }));
                drillStr += "\njoin_excellons drill " +
                            String.Join(" ", _drillFilesValid.Select((_, i) => $"drill{i}"));
            }
        }
        // milling slots
        var millingStr = "";
        // using 0.8mm to milling any slot <=2mm
        if (_drillTools.Any(o => o >= MillingDia1 && o<MillingDia2))
        {
            var sign = "08";
            var toolDia = 0.8;
            millingStr = $@"
millslots drill -milled_dias ""{MillingDia1}"" -tooldia {toolDia} -diatol 0 -outname milled_slots{sign}
cncjob milled_slots{sign} -dia {_settingsFlatCam.PcbDiameter} -z_cut -2 -dpp 0.1 -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate 100 -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname milled_slots{sign}_nc
write_gcode milled_slots{sign}_nc {dir}/milled_slots{sign}.nc
";
        }
        // using 2mm to milling any slot >2mm
        if (_drillTools.Any(o => o >= MillingDia2))
        {
            var sign = "20";
            var toolDia = 2.0;
            millingStr += $@"
millslots drill -milled_dias ""{MillingDia1}"" -tooldia {toolDia} -diatol 0 -outname milled_slots{sign}
cncjob milled_slots{sign} -dia {_settingsFlatCam.PcbDiameter} -z_cut -2 -dpp 0.2 -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate 100 -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname milled_slots{sign}_nc
write_gcode milled_slots{sign}_nc {dir}/milled_slots{sign}.nc
";
        }


        var q = "\"";


        var openTop = "";
        var processTop = "";
        if (_appOptions.Top == 1)
        {
            openTop = $"open_gerber {dir}/Gerber_TopLayer.GTL -outname top_layer";
            processTop = $@"
ncc top_layer -method Seed -tooldia {_settingsFlatCam.PcbDiameter} -overlap 25 -connect 1 -contour 1 -all -outname ncc_top
cncjob ncc_top -dia {_settingsFlatCam.PcbDiameter} -z_cut -0.1 -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate {_settingsFlatCam.XyFetchRate} -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname top_nc 
write_gcode top_nc {dir}/top_layer.nc
";
        }

        var cleanPcb = _settingsFlatCam.CleanPcb
            ? $@"
ncc mg_geo -method Seed -tooldia {_settingsFlatCam.PcbDiameter} -overlap 25 -connect 1 -contour 1 -all -outname ncc_board
cncjob ncc_board -dia {_settingsFlatCam.PcbDiameter} -z_cut -0.1 -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate {_settingsFlatCam.XyFetchRate} -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname ncc_board_nc 
write_gcode ncc_board_nc {dir}/ncc_board.nc
"
            : "";
        var scriptTemplate = $@"
set_sys {q}units{q} {q}MM{q}
open_gerber {dir}/Gerber_BoardOutlineLayer.GKO -outname cutout
open_gerber {dir}/Gerber_BottomLayer.GBL -outname bottom_layer
#open top layer gerber
{openTop}

#open and join drill files
{drillStr}
#open_excellon {dir}/Drill_PTH_Through.DRL -outname drill

mirror bottom_layer -axis Y -box cutout
mirror drill -axis Y  -box cutout

#note this must be last call on mirror
mirror cutout -axis Y  -box cutout
join_geometry mg_geo cutout bottom_layer
{cleanPcb}

#cutout
geocutout cutout -dia {_settingsFlatCam.CutOutDiameter} -gapsize 0.3 -gaps lr -outname cutout_geo
cncjob cutout_geo -dia {_settingsFlatCam.CutOutDiameter}  -dpp 0.3 -z_cut {_settingsFlatCam.DrillDepth} -z_move {_settingsFlatCam.ZClearance} -spindlespeed {_settingsFlatCam.SpindleSpeed} -feedrate {_settingsFlatCam.XyFetchRate} -feedrate_z {_settingsFlatCam.ZFetchRate} -pp default -outname cutout_nc
write_gcode cutout_nc {dir}/cutout_nc.nc

#drill

{drillGCodeExport}
{millingStr}
{processTop}

#signal application we have done job
write_gcode cutout_nc {dir}/done.pid
#quit_flatcam
";
        if (File.Exists($"{dir}/done.pid"))
        {
            File.Delete($"{dir}/done.pid");
        }

        File.WriteAllText(_appOptions.Directory + "/script-beta.txt", scriptTemplate);
       
        File.WriteAllText($"{dir}/ofs.m1s",$@"


Dim sFileText As String
Dim iFileNo As Integer
iFileNo = FreeFile
'open the file for writing
Open {q}{dir}/rec{q} For Output As #iFileNo
'please note, if this file already exists it will be overwritten!
    
Print #iFileNo,  {q}{q}
Print #iFileNo,  {q}{q}




        xd00d=GetParam({q}XMachine{q})
        yd00d=GetParam(""YMachine"")
        zd00d=GetParam(""ZMachine"")
        Print #iFileNo,""G0  X"",xd00d,"" Y"",yd00d
    

        'write some example text to the file
            Print #iFileNo,  """"


    
        'close the file (if you dont do this, you wont be able to open it again!)
        Close #iFileNo
");

        ProcessStartInfo processStartInfo = new ProcessStartInfo(_settingsFlatCam.Beta);
        processStartInfo.Arguments = $"--shellfile={dir}/script-beta.txt";
        processStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
        processStartInfo.UseShellExecute = true;
        Process process = new Process();
        process.StartInfo = processStartInfo;
        // bool done = false;
        // process.Exited += (e,v) =>
        // {
        //     done = true;
        // };


        if (!process.Start())
        {
            Console.WriteLine("Cannot spawn process");
            return;
        }

        while (!File.Exists($"{dir}/done.pid") && !process.HasExited )
        {
            Thread.Sleep(100);
        }

        while (File.Exists($"{dir}/done.pid"))
        {
            try
            {
                File.Delete($"{dir}/done.pid");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
        }
       

       
        GenerateRpf(dir);

        if (_appOptions.Kill == 1)
        {
            if (File.Exists($"{dir}/done.pid"))
            {
                File.Delete($"{dir}/done.pid");
                process.Kill();
            }
        }
        else
        {
            process.WaitForExit();
            // while (!process.HasExited)
            // {
            //     Thread.Sleep(100);
            // }
        }

        // 
    }

    public void GenerateRpf(string? dir)
    {
        var gc = _gCodeFactory.CreateNew();
        var cut = _gCodeFactory.CreateNew();
        cut.ParseFile($"{dir}/cutout_nc.nc");
        gc.ParseFile($"{dir}/ncc_board.nc");

        var min = new Point3D(Math.Min(cut.MinXyz.X, gc.MinXyz.X), Math.Min(cut.MinXyz.Y, gc.MinXyz.Y));
        var max = new Point3D(Math.Max(cut.MaxXyz.X, gc.MaxXyz.X), Math.Max(cut.MaxXyz.Y, gc.MaxXyz.Y));
        var sp = 10;
        var dx = max.X - min.X;
        var dy = max.Y - min.Y;
        var numx = (long)((dx) / sp);
        var w =dx/numx;
        var numy = (long)((dy) / sp);
        var h =dy/numy;
        StringBuilder sb = new StringBuilder();
        // var (x, y) = (min.X, max.Y);
        sb.Append($@"
G90 G21 S{_settingsFlatCam.SpindleSpeed} G17

M0 (Attach probe wires and clips that need attaching)
(Initialize probe routine)
G1 X0 Y0 F{_settingsFlatCam.XyProbeFetchRate} (Move to origin)
G31 Z-1 F{_settingsFlatCam.ZProbeFetchRate} (Probe to a maximum of the specified probe height at the specified feed rate)
G92 Z0 (Touch off Z to 0 once contact is made)
G0 Z2 (Move Z to above the contact point)
G31 Z-1 F{_settingsFlatCam.ZProbeFetchRate/2} (Repeat at a more accurate slower rate)
G92 Z0
G0 Z2
M40 (Begins a probe log file, when the window appears, enter a name for the log file such as RawProbeLog.txt)
G0 Z2");
        var rev = false;
        
        for (int i = 0; i <= numy; i++)
        {
            for (int j = 0; j <= numx; j++)
            {
                sb.Append($@"
G1 X{(min.X+w*(rev?numx-j:j)).Fmt()} Y{(min.Y+h*i).Fmt()} F{_settingsFlatCam.XyProbeFetchRate}
G31 Z-1 F{_settingsFlatCam.ZProbeFetchRate}
G0 Z2");
            }

            rev = !rev;
        }

        sb.Append($@"
M41 (Closes the opened log file)
G0 Z5
G0 X0 Y0
M0 (Detach any clips used for probing)
M30

");
        File.WriteAllText($"{dir}/rpf.nc",sb.ToString());
    }


    private void ProcessTool(FileTypeAct fileTypeAct)
    {
        var allLines = File.ReadLines(fileTypeAct.FileName).ToList();
        var tools = new List<double>();

        for (int i = 0; i < allLines.Count(); i++)
        {
            var str = allLines[i];
            if (str.Trim().Equals("%")) break;
            var m = Regex.Match(str, @"^T\d+C(\d+\.?\d+)");
            if (m.Success)
            {
                var tool = double.Parse(m.Groups[1].Value);
                if (tool < 0.8)
                {
                    tool = 0.6;
                }
                else if (tool < MillingDia1)
                {
                    tool = 0.8;
                }
                else if (tool < MillingDia2)
                    tool = MillingDia1;
                else tool = MillingDia2;
                

                tools.Add(tool);
                allLines[i] = Regex.Replace(str, m.Groups[1].Value, String.Format("{0:0.0000}", tool));
            }
        }

        if (allLines.Any(s => s[0] == 'X'))
        {
            _drillTools.AddRange(tools);
            _drillFilesValid.Add(fileTypeAct.FileName);
        }

        File.WriteAllLines(fileTypeAct.FileName, allLines);
    }
}

public class FileTypeAct
{
    public string FileName { get; set; } = "";
    public string Ext { get; set; } = "";
}