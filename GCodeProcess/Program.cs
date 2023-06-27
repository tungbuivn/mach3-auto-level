using GCodeProcess;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
// Ask the service provider for the configuration abstraction.

builder.RegisterIoc(args);


// builder.Services.AddTransient<ServiceLifetimeReporter>();

using IHost host = builder.Build();

host.Services.CreateScope().ServiceProvider.GetRequiredService<AppHandler>().Run();


// await host.RunAsync();
