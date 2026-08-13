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

        DownloadPath = Path.Combine(Path.GetTempPath(), Settings.DownloadPath ?? "epam-downloads");
        Directory.CreateDirectory(DownloadPath);

        LoggingManager.Instance.Initialize(Settings.Logging);
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}

public abstract class BaseTest : IAsyncLifetime
{
    protected IWebDriver Driver { get; private set; } = null!;
    protected WebDriverWait Wait { get; private set; } = null!;
    protected ILogger Logger { get; }
    protected TestSettings Settings { get; }
    protected string DownloadPath { get; }

    protected BaseTest()
    {
        var environment = TestEnvironmentFixture.Instance;

        Settings = environment.Settings;
        DownloadPath = environment.DownloadPath;

        Logger = Log.ForContext(GetType());
        Logger.Information("Starting test class: {TestClass}", GetType().Name);
        Logger.Information("Initializing browser: {Browser}", Settings.Browser);
    }

    public ValueTask InitializeAsync()
    {
        Driver = BrowserFactory.Create(Settings.Browser, DownloadPath);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        Logger.Information("Navigating to base URL: {BaseUrl}", Settings.BaseUrl);
        Driver.Navigate().GoToUrl(Settings.BaseUrl);

        BrowserFactory.DismissOneTrustCookies(Driver);
        var js = (IJavaScriptExecutor)Driver;
        js.ExecuteScript(@"
            const banner = document.getElementById('onetrust-banner-sdk');
            if (banner) { banner.style.display = 'none'; }
            const backdrop = document.getElementById('onetrust-pc-sdk');
            if (backdrop) { backdrop.style.display = 'none'; }
        ");

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        try
        {
            var state = TestContext.Current?.TestState;
            if (state?.Result == TestResult.Failed)
            {
                var testName = TestContext.Current?.Test?.TestDisplayName ?? "UnknownTest";
                var message = state.ExceptionMessages?.FirstOrDefault();
                Logger.Error("Test '{TestName}' failed: {Message}", testName, message);
                Core.TestUtils.TakeScreenshot(Driver, Logger, testName, GetType().Name);
            }

            Logger.Information("Closing browser");
            Driver?.Quit();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during browser cleanup");
        }

        return ValueTask.CompletedTask;
    }
}