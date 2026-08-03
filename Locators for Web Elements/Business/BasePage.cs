using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace Locators_for_Web_Elements.Business;

public class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly Actions Actions;

    public BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        Actions = new Actions(driver);
    }

    public void DismissOneTrust()
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
        catch (Exception ex)
        {
            Debug.WriteLine($"DismissOneTrust failed: {ex.Message}");
        }
    }
}
