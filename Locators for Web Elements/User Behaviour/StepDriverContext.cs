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

        ArtifactsRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", settings.ArtifactsRoot ?? "TestResults/artifacts"));
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
        Logger.Information("Navigating to base URL: {BaseUrl}", BaseUrl);
        Driver.Navigate().GoToUrl(BaseUrl);
        BrowserFactory.DismissOneTrustCookies(Driver);
        Logger.Information("OneTrust cookies dismissed");
    }

    public void Dispose()
    {
        Logger.Information("Closing browser for scenario");
        Driver?.Quit();
    }
}
