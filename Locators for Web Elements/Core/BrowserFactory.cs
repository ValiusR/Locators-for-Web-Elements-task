using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;

namespace Locators_for_Web_Elements.Core;

public static class BrowserFactory
{
    public static IWebDriver Create(string browser, string downloadPath, BrowserOptions options)
    {
        var browserName = browser.ToLowerInvariant();

        return browserName switch
        {
            "chrome" => CreateChromeDriver(downloadPath, options),
            "firefox" => CreateFirefoxDriver(downloadPath, options),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };
    }

    private static IWebDriver CreateChromeDriver(string downloadPath, BrowserOptions options)
    {
        var chromeOptions = CreateChromeOptions(downloadPath, options);
        return new ChromeDriver(chromeOptions);
    }

    private static ChromeOptions CreateChromeOptions(string downloadPath, BrowserOptions options)
    {
        var chromeOptions = new ChromeOptions();

        if (options.UserDataDir)
        {
            var userDataDir = Path.Combine(Path.GetTempPath(), $"epam-chrome-profile-{Guid.NewGuid():N}");
            Directory.CreateDirectory(userDataDir);
            chromeOptions.AddArgument($"--user-data-dir={userDataDir}");
        }

        if (options.DisableInfoBars)
            chromeOptions.AddArgument("--disable-infobars");

        if (options.DisableDevShmUsage)
            chromeOptions.AddArgument("--disable-dev-shm-usage");

        if (options.NoSandbox)
            chromeOptions.AddArgument("--no-sandbox");

        if (options.Headless)
            chromeOptions.AddArgument("--headless=new");

        chromeOptions.AddUserProfilePreference("intl.accept_languages", options.Language);
        chromeOptions.AddUserProfilePreference("download.prompt_for_download", options.DownloadPrompt);
        chromeOptions.AddUserProfilePreference("download.default_directory", downloadPath);
        chromeOptions.AddUserProfilePreference("download.directory_upgrade", options.DirectoryUpgrade);
        chromeOptions.AddUserProfilePreference("plugins.always_open_pdf_externally", options.AlwaysOpenPdfExternally);

        return chromeOptions;
    }

    private static IWebDriver CreateFirefoxDriver(string downloadPath, BrowserOptions options)
    {
        var firefoxOptions = CreateFirefoxOptions(downloadPath, options);
        var service = FirefoxDriverService.CreateDefaultService();
        return new FirefoxDriver(service, firefoxOptions);
    }

    private static FirefoxOptions CreateFirefoxOptions(string downloadPath, BrowserOptions options)
    {
        var firefoxOptions = new FirefoxOptions();

        firefoxOptions.SetPreference("intl.accept_languages", options.Language);
        firefoxOptions.SetPreference("browser.download.folderList", 2);
        firefoxOptions.SetPreference("browser.download.dir", downloadPath);
        firefoxOptions.SetPreference("browser.download.prompt_for_download", options.DownloadPrompt);
        firefoxOptions.SetPreference("pdfjs.disabled", true);

        if (options.Headless)
            firefoxOptions.AddArgument("-headless");

        return firefoxOptions;
    }
}
