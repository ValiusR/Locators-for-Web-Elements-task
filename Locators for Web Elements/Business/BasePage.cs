using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Locators_for_Web_Elements.Business;

public class BasePage
{
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;
    protected readonly Actions Actions;
    protected readonly ILogger Logger;

    public BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
        Actions = new Actions(driver);
        Logger = Log.ForContext(GetType());

        DismissOneTrust();
    }
    public void DismissOneTrust()
    {
        Locators_for_Web_Elements.Core.BrowserFactory.DismissOneTrustCookies(Driver);
        Logger.Information("OneTrust cookies dismissed");
    }
}
