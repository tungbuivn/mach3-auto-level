using System.Diagnostics;
using CommandLine;
using GCodeProcess.FlatCam;
using GCodeProcess.Fusion360;
using GCodeProcess.GCode;
using GCodeProcess.Gerber;
using GCodeProcess.HeightMap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GCodeProcess;

public static class IoC
{
    public static Func<string[],AppOptions> GetOptions = (args) =>
    {
        var ps = new Parser(ops => { ops.IgnoreUnknownArguments = true; });
        var opts = ps.ParseArguments<AppOptions>(args).WithNotParsed((er) => { })
            .MapResult(rs =>
            {
                // AppOptions rs = null;
                if (rs.Cmd == "ger")
                {
                    rs = Parser.Default.ParseArguments<GerberOptions>(args).Value;
                }
                else if (rs.Cmd == "360")
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
        return opts;
    };
    public static void RegisterIoc(this HostApplicationBuilder builder, string[] args)
    {
        RegisterIoc(builder.Services, args);
    }

    public static void RegisterIoc(this IServiceCollection serviceCollection, string[] args)
    {
        IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

// Get values from the config given their key and their target type.
        Settings settings = config.GetRequiredSection("Settings").Get<Settings>() ??
                            throw new InvalidOperationException();

        serviceCollection.AddTransient<GCode.GCode>();
        serviceCollection.AddSingleton<GCodeFactory>();
        serviceCollection.AddSingleton<DrillFileParser>();
        serviceCollection.AddSingleton(settings);
        serviceCollection.AddTransient<FlatCam.FlatCam>();
        serviceCollection.AddTransient<Fusion360.Fusion360>();
        serviceCollection.AddTransient<HeightMap.HeightMap>();
        serviceCollection.AddSingleton<AppHandler>();
        
        
        // AppOptions opts = Parser.Default.ParseArguments<AppOptions>(args).Value;
        
            serviceCollection.AddTransient((svc)=>GetOptions(args));
    }
}