using System.IO;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests;

public abstract class BaseTest : IAsyncLifetime
{
    protected IWebDriver Driver { get; private set; } = null!;
    protected WebDriverWait Wait { get; private set; } = null!;
    protected ILogger Logger { get; }
    protected TestSettings Settings { get; }
    protected string DownloadPath { get; private set; } = null!;

    protected BaseTest()
    {
        Logger = Log.ForContext(GetType());

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

        if (!LoggingManager.Instance.IsInitialized)
            LoggingManager.Instance.Initialize(Settings.Logging);

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

        BrowserFactory.DismissOneTrustCookies(Driver);

        Logger.Information("Navigating to base URL: {BaseUrl}", Settings.BaseUrl);
        Driver.Navigate().GoToUrl(Settings.BaseUrl);

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        var state = TestContext.Current.TestState;
        if (state?.Result == TestResult.Failed)
        {
            var testName = TestContext.Current.Test?.TestDisplayName ?? "UnknownTest";
            var message = state.ExceptionMessages?.FirstOrDefault();
            Logger.Error("Test '{TestName}' failed: {Message}", testName, message);
            Core.TestUtils.TakeScreenshot(Driver, Logger, testName, GetType().Name);
        }

        try
        {
            Logger.Information("Closing browser");
            Driver?.Quit();
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Error during browser cleanup");
        }
        finally
        {
            Log.CloseAndFlush();
        }

        return ValueTask.CompletedTask;
    }

}
