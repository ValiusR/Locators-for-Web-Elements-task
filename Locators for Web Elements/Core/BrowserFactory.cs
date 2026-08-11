using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Locators_for_Web_Elements.Core;

public static class BrowserFactory
{
    public static IWebDriver Create(string browser, string downloadPath)
    {
        var browserName = browser.ToLowerInvariant();

        return browserName switch
        {
            "chrome" => CreateChromeDriver(downloadPath),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };
    }

    private static IWebDriver CreateChromeDriver(string downloadPath)
    {
        var options = CreateChromeOptions(downloadPath);
        return new ChromeDriver(options);
    }

    private static ChromeOptions CreateChromeOptions(string downloadPath)
    {
        var options = new ChromeOptions();
        var userDataDir = Path.Combine(Path.GetTempPath(), "epam-chrome-profile");

        options.AddArgument($"--user-data-dir={userDataDir}");
        options.AddArgument("--disable-infobars");
        options.AddUserProfilePreference("intl.accept_languages", "en-US");
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("download.default_directory", downloadPath);
        options.AddUserProfilePreference("download.directory_upgrade", true);
        options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

        return options;
    }

    public static void DismissOneTrustCookies(IWebDriver driver)
    {
        if (driver is ChromeDriver chrome)
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
}
