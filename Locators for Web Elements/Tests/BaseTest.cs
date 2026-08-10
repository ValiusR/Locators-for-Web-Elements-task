using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Serilog;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests;

public abstract class BaseTest : IDisposable
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly ILogger Logger;
    protected readonly string BaseUrl;
    protected readonly string DownloadPath;

    protected BaseTest()
    {
        Logger = Log.ForContext(GetType());

        var environment = Environment.GetEnvironmentVariable("TAF_ENVIRONMENT") ?? "Production";

        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("Tests/config.json", optional: false)
            .AddJsonFile($"Tests/config.{environment}.json", optional: true)
            .Build();

        if (!LoggingManager.Instance.IsInitialized)
            LoggingManager.Instance.Initialize(config);

        BaseUrl = config["BaseUrl"]!;
        DownloadPath = Path.Combine(Path.GetTempPath(), config["DownloadPath"] ?? "epam-downloads");
        Directory.CreateDirectory(DownloadPath);

        Logger.Information("Starting test class: {TestClass}", GetType().Name);
        Logger.Information("Initializing browser: {Browser}", config["Browser"] ?? "Chrome");

        var options = new ChromeOptions();
        var userDataDir = Path.Combine(Path.GetTempPath(), "epam-chrome-profile");
        options.AddArgument($"--user-data-dir={userDataDir}");
        options.AddArgument("--disable-infobars");
        options.AddUserProfilePreference("intl.accept_languages", "en-US");
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("download.default_directory", DownloadPath);
        options.AddUserProfilePreference("download.directory_upgrade", true);
        options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

        Driver = BrowserFactory.Create(config["Browser"] ?? "Chrome", options);
        Driver.Manage().Window.Maximize();
        Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        Wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

        DismissOneTrustCookies();

        Logger.Information("Navigating to base URL: {BaseUrl}", BaseUrl);
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
                Logger.Information("OneTrust consent cookies set");
            }
        }
        catch (Exception ex)
        {
            Logger.Warning(ex, "Failed to dismiss OneTrust cookies");
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
            Logger.Error(ex, "Test '{TestName}' failed", testName);
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
                Logger.Error("Screenshot saved: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to capture screenshot for test: {TestName}", testName);
        }
    }

    protected string WaitForFileDownload(string partialFileName, int timeoutSeconds = 30)
    {
        Logger.Information("Waiting for file download: {FileName}", partialFileName);
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var file = Directory.GetFiles(DownloadPath)
                .FirstOrDefault(f => Path.GetFileName(f).Contains(partialFileName));
            return file;
        })!;
    }

    public void Dispose()
    {
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
    }
}
