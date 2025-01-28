using System.IO;
using Serilog;
using Serilog.Events;

namespace GitActionRunner.Converters;

public static class LoggerSetup
{
    public static void Configure(LogEventLevel minimumLevel)
    {
        CleanUp();
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
        Directory.CreateDirectory(logPath);

        var logFile = Path.Combine(logPath, $"log-{DateTime.Now:yyyy-MM-dd}.txt");

        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Is(minimumLevel)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(logFile,
                              rollingInterval: RollingInterval.Day,
                              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

        Log.Information("Logger initialization completed - logging level: \"{Level}\"", minimumLevel);
    }

    public static void CleanUp()
    {
        Log.CloseAndFlush();
    }
}