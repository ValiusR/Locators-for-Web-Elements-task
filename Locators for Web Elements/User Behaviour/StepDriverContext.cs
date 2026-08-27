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

    private static string GetProjectRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, ".git")))
        {
            directory = directory.Parent;
        }
        return directory?.FullName ?? AppContext.BaseDirectory;
    }

    public StepDriverContext()
    {
        var settings = TestEnvironmentFixture.Instance.Settings;
        DownloadPath = Path.Combine(Path.GetTempPath(), settings.DownloadPath ?? "downloads");
        Directory.CreateDirectory(DownloadPath);

        var projectRoot = GetProjectRoot();
        ArtifactsRoot = Path.GetFullPath(Path.Combine(projectRoot, settings.ArtifactsRoot ?? "TestResults/artifacts"));
        Directory.CreateDirectory(ArtifactsRoot);

    Logger = Log.ForContext<StepDriverContext>();
    Logger.Information("Creating WebDriver for scenario. Artifacts root: {ArtifactsRoot}", ArtifactsRoot);

    Driver = BrowserFactory.Create(settings.Browser, DownloadPath, settings.BrowserOptions);
    Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

    Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
    Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

    BaseUrl = settings.BaseUrl;
    }

    public void NavigateToHomePage()
    {
        var projectRoot = GetProjectRoot();
        var settings = TestEnvironmentFixture.Instance.Settings;

        Logger.Information("Navigating to base URL: {BaseUrl}", BaseUrl);
        Driver.Navigate().GoToUrl(BaseUrl);
        ConsentHelper.DismissOneTrustCookies(Driver);
        Logger.Information("OneTrust cookies dismissed");

        var debugDir = Path.Combine(ArtifactsRoot, "debug");
        Directory.CreateDirectory(debugDir);

        if (Driver is ITakesScreenshot screenshotDriver)
        {
            var screenshot = screenshotDriver.GetScreenshot();
            var filePath = Path.Combine(debugDir, $"homepage_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
            screenshot.SaveAsFile(filePath);
            Logger.Information("Debug screenshot saved: {FilePath}", filePath);
        }

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
