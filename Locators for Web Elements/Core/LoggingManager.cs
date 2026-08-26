using Serilog;
using Serilog.Events;

namespace Locators_for_Web_Elements.Core;

public sealed class LoggingManager
{
    private static LoggingManager? _instance;
    private static readonly object _lock = new();

    public static LoggingManager Instance
    {
        get
        {
            lock (_lock)
            {
                _instance ??= new LoggingManager();
                return _instance;
            }
        }
    }

    public bool IsInitialized { get; private set; }

    private LoggingManager() { }

    public void Initialize(LoggingSettings loggingSettings)
    {
        var minLevelStr = loggingSettings.MinLevel;
        var filePath = loggingSettings.FilePath;
        var consoleOutput = loggingSettings.ConsoleOutput;
        var retainedCount = loggingSettings.RetainedFileCountLimit;

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
