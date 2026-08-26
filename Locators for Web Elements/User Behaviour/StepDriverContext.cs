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

    public StepDriverContext()
    {
        var settings = TestEnvironmentFixture.Instance.Settings;
        DownloadPath = Path.Combine(Path.GetTempPath(), settings.DownloadPath ?? "downloads");
        Directory.CreateDirectory(DownloadPath);

        ArtifactsRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), settings.ArtifactsRoot ?? "TestResults/artifacts"));
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
        var cwd = Directory.GetCurrentDirectory();
        var settings = TestEnvironmentFixture.Instance.Settings;

        // Write debug-info.txt BEFORE navigation so it's always captured,
        // even if navigation or element lookup fails later
        try
        {
            var debugInfo = string.Join("\n", new[]
            {
                $"CWD={cwd}",
                $"ArtifactsRoot={ArtifactsRoot}",
                $"BaseUrl={BaseUrl}",
                $"Browser={settings.Browser}",
                $"Headless={settings.BrowserOptions.Headless}",
                $"PageUrl=(not yet navigated)",
                $"PageTitle=(not yet navigated)",
                $"PageSourceLength=0"
            });
            var debugPath = Path.Combine(cwd, "TestResults", "debug-info.txt");
            File.WriteAllText(debugPath, debugInfo);
            Trace.WriteLine($"[DEBUG] Pre-navigation info written: {debugPath}");
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[DEBUG] Failed to write pre-navigation debug info: {ex.Message}");
        }

        Trace.WriteLine($"[DEBUG] CWD={cwd} ArtifactsRoot={ArtifactsRoot}");
        Logger.Information("Navigating to base URL: {BaseUrl}", BaseUrl);
        Driver.Navigate().GoToUrl(BaseUrl);
        ConsentHelper.DismissOneTrustCookies(Driver);
        Logger.Information("OneTrust cookies dismissed");

        try
        {
            var debugInfo = string.Join("\n", new[]
            {
                $"CWD={cwd}",
                $"ArtifactsRoot={ArtifactsRoot}",
                $"BaseUrl={BaseUrl}",
                $"Browser={settings.Browser}",
                $"Headless={settings.BrowserOptions.Headless}",
                $"PageUrl={Driver.Url}",
                $"PageTitle={Driver.Title}",
                $"PageSourceLength={Driver.PageSource.Length}"
            });
            var debugPath = Path.Combine(cwd, "TestResults", "debug-info.txt");
            File.WriteAllText(debugPath, debugInfo);
            Trace.WriteLine($"[DEBUG] Post-navigation info written: {debugPath}");
            Logger.Information("Debug info written: {Path}", debugPath);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[DEBUG] Failed to write post-navigation debug info: {ex.Message}");
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
            Trace.WriteLine($"[DEBUG] Page source saved: {sourcePath}");
            Logger.Information("Page source saved: {SourcePath}", sourcePath);
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"[DEBUG] Failed to save page source: {ex.Message}");
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
