using OpenQA.Selenium;
using Serilog;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests.StepDefinitions;

public sealed class StepDriverContext : IDisposable
{
    public IWebDriver Driver { get; }
    public string BaseUrl { get; }
    public string DownloadPath { get; }

    public StepDriverContext()
    {
        var settings = TestEnvironmentFixture.Instance.Settings;
        DownloadPath = Path.Combine(Path.GetTempPath(), settings.DownloadPath ?? "downloads");
        Directory.CreateDirectory(DownloadPath);

        Driver = BrowserFactory.Create(settings.Browser, DownloadPath);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        BaseUrl = settings.BaseUrl;
    }

    public void NavigateToHomePage()
    {
        Driver.Navigate().GoToUrl(BaseUrl);
        BrowserFactory.DismissOneTrustCookies(Driver);
    }

    public void Dispose()
    {
        try
        {
            Driver?.Quit();
        }
        catch (Exception ex) when (ex is WebDriverException or InvalidOperationException)
        {
            Log.Warning(ex, "Browser cleanup failed during scenario disposal.");
        }
    }
}