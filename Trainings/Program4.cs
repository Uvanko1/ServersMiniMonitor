using Serilog;

public class Program4
{
    static async Task Main()
    {
        var builder = Host.CreateApplicationBuilder();
        var servers = builder.Configuration.GetSection("Servers").Get<List<Server>>();
        builder.Services.AddSingleton<List<Server>>(servers);
        builder.Services.AddHostedService<MonitorWorker>();
        Log.Logger = new LoggerConfiguration().WriteTo.Console().WriteTo.File("monitor.log").CreateLogger();
        builder.Services.AddSerilog();
        builder.Services.AddSingleton<IMonitorService, MonitorService>();
        builder.Services.AddHttpClient();
        var host = builder.Build();
        var running = host.RunAsync();
        Console.ReadKey();
        await host.StopAsync();
        await running;
    }
}
