using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace Locators_for_Web_Elements.Core;

public abstract class BaseTest : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly string BaseUrl;
    protected readonly string DownloadPath;

    protected BaseTest()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json")
            .Build();

        BaseUrl = config["BaseUrl"]!;
        DownloadPath = Path.Combine(Path.GetTempPath(), config["DownloadPath"] ?? "epam-downloads");
        Directory.CreateDirectory(DownloadPath);

        var options = new ChromeOptions();
        var userDataDir = Path.Combine(Path.GetTempPath(), "epam-chrome-profile");
        options.AddArgument($"--user-data-dir={userDataDir}");
        options.AddArgument("--disable-infobars");
        options.AddUserProfilePreference("intl.accept_languages", "en-US");
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("download.default_directory", DownloadPath);
        options.AddUserProfilePreference("download.directory_upgrade", true);
        options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

        Driver = new ChromeDriver(options);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        DismissOneTrustCookies();

        Driver.Navigate().GoToUrl(BaseUrl);
    }

    private void DismissOneTrustCookies()
    {
        try
        {
            if (Driver is ChromeDriver chrome)
            {
                var cookieNames = new[] { "OptanonAlertBoxClosed", "onetrust-consent-sent" };
                foreach (var name in cookieNames)
                {
                    chrome.ExecuteCdpCommand("Network.setCookie", new Dictionary<string, object?>
                    {
                        ["name"] = name,
                        ["value"] = "true",
                        ["domain"] = ".epam.com",
                        ["path"] = "/"
                    });
                }
            }
        }
        catch
        {
        }
    }

    protected void ExecuteTest(Action testBody, string testName)
    {
        try
        {
            testBody();
        }
        catch (Exception ex)
        {
            TakeScreenshot(testName);
            throw;
        }
    }

    protected void TakeScreenshot(string testName)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var screenshotDir = Path.Combine("Screenshots", GetType().Name);
            Directory.CreateDirectory(screenshotDir);
            var filePath = Path.Combine(screenshotDir, $"{testName}_{timestamp}.png");

            if (Driver is ITakesScreenshot screenshotDriver)
            {
                var screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(filePath);
            }
        }
        catch
        {
        }
    }

    public void Dispose()
    {
        try
        {
            Driver?.Quit();
        }
        catch
        {
        }
    }
}
