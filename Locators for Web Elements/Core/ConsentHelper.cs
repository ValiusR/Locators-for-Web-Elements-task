using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace Locators_for_Web_Elements.Core;

public static class ConsentHelper
{
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

            var script = @"
                if (window.OneTrust !== undefined) {
                    OneTrust.NoticeCallback = function() {};
                }
                localStorage.setItem('OptanonAlertBoxClosed', 'true');
                localStorage.setItem('onetrust-consent-sent', 'true');
            ";
            ((IJavaScriptExecutor)driver).ExecuteScript(script);
        }
    }
}
