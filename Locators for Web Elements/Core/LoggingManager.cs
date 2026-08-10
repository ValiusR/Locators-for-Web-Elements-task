using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;

namespace Locators_for_Web_Elements.Core;

public sealed class LoggingManager
{
    private static readonly Lazy<LoggingManager> _instance = new(() => new LoggingManager());

    public static LoggingManager Instance => _instance.Value;

    public bool IsInitialized { get; private set; }

    private LoggingManager() { }

    public void Initialize(IConfiguration configuration)
    {
        if (IsInitialized) return;

        var logSection = configuration.GetSection("Logging");
        var minLevelStr = logSection["MinLevel"] ?? "Information";
        var filePath = logSection["FilePath"] ?? "Logs/epam-taf-.log";
        var consoleOutput = bool.Parse(logSection["ConsoleOutput"] ?? "true");
        var retainedCount = int.Parse(logSection["RetainedFileCountLimit"] ?? "7");

        var loggerConfig = new LoggerConfiguration();

        if (Enum.TryParse<LogEventLevel>(minLevelStr, true, out var minLevel))
            loggerConfig.MinimumLevel.Is(minLevel);
        else
            loggerConfig.MinimumLevel.Information();

        if (consoleOutput)
            loggerConfig.WriteTo.Console();

        loggerConfig.WriteTo.File(
            path: filePath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: retainedCount,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        );

        Log.Logger = loggerConfig.CreateLogger();
        IsInitialized = true;
    }
}
