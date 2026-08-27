using System.IO;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Xunit;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests;

public sealed class TestEnvironmentFixture : IDisposable
{
    public static TestEnvironmentFixture Instance { get; private set; } = new();

    public TestSettings Settings { get; }
    public string DownloadPath { get; }

    public TestEnvironmentFixture()
    {
        var environment = Environment.GetEnvironmentVariable("TAF_ENVIRONMENT") ?? "Production";
        Settings = LoadSettings(environment);

        var browserEnv = Environment.GetEnvironmentVariable("BROWSER");
        if (!string.IsNullOrWhiteSpace(browserEnv))
        {
            Settings.Browser = browserEnv;
        }

        DownloadPath = Path.Combine(Path.GetTempPath(), Settings.DownloadPath ?? "epam-downloads");
        Directory.CreateDirectory(DownloadPath);

        LoggingManager.Instance.Initialize(Settings.Logging);

        // Guarantee flush even if xUnit process terminates abruptly
        AppDomain.CurrentDomain.ProcessExit += (_, _) => Log.CloseAndFlush();
    }
    [CollectionDefinition("TestEnvironment")]
    public class TestEnvironmentCollection : ICollectionFixture<TestEnvironmentFixture> { }

    private static TestSettings LoadSettings(string environment)
    {
        var candidates = new[]
        {
            Path.Combine(AppContext.BaseDirectory, "Tests", "config.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "Tests", "config.json"),
            Path.Combine(AppContext.BaseDirectory, "..", "..", "Tests", "config.json"),
        };

        string? configPath = candidates.FirstOrDefault(File.Exists);
        if (configPath == null)
        {
            var tried = string.Join("\n", candidates);
            throw new FileNotFoundException(
                $"config.json not found. Tried:\n{tried}\nAppContext.BaseDirectory={AppContext.BaseDirectory}");
        }

        var json = File.ReadAllText(configPath);
        var settings = JsonSerializer.Deserialize<TestSettings>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Failed to deserialize TestSettings");

        if (!string.IsNullOrEmpty(environment) && environment != "Production")
        {
            var envPath = Path.Combine(Path.GetDirectoryName(configPath)!, $"config.{environment}.json");
            if (File.Exists(envPath))
            {
                var envJson = File.ReadAllText(envPath);
                var envSettings = JsonSerializer.Deserialize<TestSettings>(envJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (envSettings != null)
                {
                    var envProps = typeof(TestSettings).GetProperties();
                    foreach (var prop in envProps)
                    {
                        var envValue = prop.GetValue(envSettings);
                        if (envValue != null && !(envValue is string s && string.IsNullOrEmpty(s)))
                        {
                            prop.SetValue(settings, envValue);
                        }
                    }
                }
            }
        }

        return settings;
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}
