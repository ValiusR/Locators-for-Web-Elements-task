using System.IO;
using Microsoft.Extensions.Configuration;
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
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Tests/config.json", optional: false)
            .AddJsonFile($"Tests/config.{environment}.json", optional: true)
            .Build();

        Settings = new TestSettings();
        config.Bind(Settings);

        var browserEnv = Environment.GetEnvironmentVariable("BROWSER");
        if (!string.IsNullOrWhiteSpace(browserEnv))
        {
            Settings.Browser = browserEnv;
        }

        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI")))
        {
            Settings.BrowserOptions.Headless = true;
        }

        DownloadPath = Path.Combine(Path.GetTempPath(), Settings.DownloadPath ?? "epam-downloads");
        Directory.CreateDirectory(DownloadPath);

        LoggingManager.Instance.Initialize(Settings.Logging);
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}
