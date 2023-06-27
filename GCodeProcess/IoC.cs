using System.Diagnostics;
using CommandLine;
using GCodeProcess.FlatCam;
using GCodeProcess.Fusion360;
using GCodeProcess.GCode;
using GCodeProcess.HeightMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GCodeProcess;

public static class IoC
{
    public static void RegisterIoc(this HostApplicationBuilder builder, string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

// Get values from the config given their key and their target type.
        Settings settings = config.GetRequiredSection("Settings").Get<Settings>() ??
                            throw new InvalidOperationException();

        builder.Services.AddTransient<GCode.GCode>();
        builder.Services.AddSingleton<GCodeFactory>();
        builder.Services.AddSingleton(settings);
        builder.Services.AddSingleton<FlatCam.FlatCam>();
        builder.Services.AddSingleton<Fusion360.Fusion360>();
        builder.Services.AddSingleton<HeightMap.HeightMap>();
        builder.Services.AddSingleton<AppHandler>();
        var ps = new Parser(ops =>
        {
            ops.IgnoreUnknownArguments = true;
        });
       var opts= ps.ParseArguments<AppOptions>(args).WithNotParsed((er)=>{})
           .MapResult(rs =>
        {
            // AppOptions rs = null;
            if (rs.Cmd == "ger")
            {
                rs= Parser.Default.ParseArguments<GerberOptions>(args).Value;
            }
            else  if (rs.Cmd == "360")
            {
                rs = Parser.Default.ParseArguments<Fusion360Options>(args).Value;
            }
            else if (rs.Cmd == "map")
            {
                rs = Parser.Default.ParseArguments<HeightMapOptions>(args).Value;
            }
            
            return rs;
            // return rs;
        }, (err) => null!);
        // AppOptions opts = Parser.Default.ParseArguments<AppOptions>(args).Value;
      
        builder.Services.AddSingleton(opts);

        // switch (opts.Cmd)
        // {
        //     case "ger":
        //         builder.Services.AddSingleton(Parser.Default.ParseArguments<GerberOptions>(args).Value);
        //         break;
        //     case "360":
        //         builder.Services.AddSingleton(Parser.Default.ParseArguments<Fusion360Options>(args).Value);
        //
        //         break;
        //     case "map":
        //         builder.Services.AddSingleton(Parser.Default.ParseArguments<HeightMapOptions>(args).Value);
        //
        //         break;
        // }
    }
}