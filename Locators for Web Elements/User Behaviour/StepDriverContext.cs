using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Locators_for_Web_Elements.Core;
using Locators_for_Web_Elements.Tests;

namespace Locators_for_Web_Elements.User_Behaviour;

public sealed class StepDriverContext : IDisposable
{
    public IWebDriver Driver { get; }
    public WebDriverWait Wait { get; }
    public string BaseUrl { get; }
    public string DownloadPath { get; }
    public string ArtifactsRoot { get; }
    public ILogger Logger { get; }
    private readonly string _projectRoot;

    public StepDriverContext()
    {
        var settings = TestEnvironmentFixture.Instance.Settings;
        DownloadPath = Path.Combine(Path.GetTempPath(), settings.DownloadPath ?? "downloads");
        Directory.CreateDirectory(DownloadPath);

        _projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));
        ArtifactsRoot = Path.GetFullPath(Path.Combine(_projectRoot, settings.ArtifactsRoot ?? "TestResults/artifacts"));
        Directory.CreateDirectory(ArtifactsRoot);

        Logger = Log.ForContext<StepDriverContext>();
        Logger.Information("Creating WebDriver for scenario. Artifacts root: {ArtifactsRoot}", ArtifactsRoot);

        Driver = BrowserFactory.Create(settings.Browser, DownloadPath, settings.BrowserOptions);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        BaseUrl = settings.BaseUrl;
    }

    public void NavigateToHomePage()
    {
        Console.WriteLine($"[DEBUG] CWD={Directory.GetCurrentDirectory()} ArtifactsRoot={ArtifactsRoot}");
        Logger.Information("Navigating to base URL: {BaseUrl}", BaseUrl);
        Driver.Navigate().GoToUrl(BaseUrl);
        ConsentHelper.DismissOneTrustCookies(Driver);
        Logger.Information("OneTrust cookies dismissed");

        try
        {
            var debugInfo = string.Join("\n", new[]
            {
                $"CWD={Directory.GetCurrentDirectory()}",
                $"ArtifactsRoot={ArtifactsRoot}",
                $"BaseUrl={BaseUrl}",
                $"Browser={TestEnvironmentFixture.Instance.Settings.Browser}",
                $"Headless={TestEnvironmentFixture.Instance.Settings.BrowserOptions.Headless}",
                $"PageUrl={Driver.Url}",
                $"PageTitle={Driver.Title}",
                $"PageSourceLength={Driver.PageSource.Length}"
            });
            var debugPath = Path.Combine(_projectRoot, "TestResults", "debug-info.txt");
            File.WriteAllText(debugPath, debugInfo);
            Logger.Information("Debug info written: {Path}", debugPath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to write debug info");
        }

        var debugDir = Path.Combine(ArtifactsRoot, "debug");
        Directory.CreateDirectory(debugDir);

        TestUtils.TakeScreenshot(
            Driver,
            Logger,
            "homepage",
            "debug",
            debugDir
        );

        try
        {
            var pageSource = Driver.PageSource;
            var sourcePath = Path.Combine(debugDir, $"homepage_{DateTime.Now:yyyyMMdd_HHmmss_fff}.html");
            File.WriteAllText(sourcePath, pageSource);
            Logger.Information("Page source saved: {SourcePath}", sourcePath);
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save page source");
        }

        Logger.Information("Debug capture saved after homepage navigation");
    }

    public void Dispose()
    {
        Logger.Information("Closing browser for scenario");
        Driver?.Quit();
    }
}
