using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Locators_for_Web_Elements.Core;

public static class BrowserFactory
{
    public static IWebDriver Create(string browser, ChromeOptions? options = null)
    {
        return browser.ToLowerInvariant() switch
        {
            "chrome" => new ChromeDriver(options ?? new ChromeOptions()),
            _ => throw new ArgumentException($"Unsupported browser: {browser}")
        };
    }
}
