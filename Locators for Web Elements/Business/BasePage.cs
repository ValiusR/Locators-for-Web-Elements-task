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

        if (IsOneTrustBannerVisible(Driver))
        {
            DismissOneTrust();
        }
    }

    public static bool IsOneTrustBannerVisible(IWebDriver driver)
    {
        try
        {
            var banner = driver.FindElement(By.Id("onetrust-banner-sdk"));
            if (banner == null)
            {
                return false;
            }

            var display = banner.GetCssValue("display");
            var visibility = banner.GetCssValue("visibility");
            var opacity = banner.GetCssValue("opacity");

            return banner.Displayed
                && !string.Equals(display, "none", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(visibility, "hidden", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(opacity, "0", StringComparison.OrdinalIgnoreCase);
        }
        catch (NoSuchElementException)
        {
            return false;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    public void DismissOneTrust()
    {
        Locators_for_Web_Elements.Core.BrowserFactory.DismissOneTrustCookies(Driver);
        Logger.Information("OneTrust cookies dismissed");
    }
}
