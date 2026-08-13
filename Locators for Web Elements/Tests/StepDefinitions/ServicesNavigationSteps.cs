using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Reqnroll;
using Serilog;
using Locators_for_Web_Elements.Business;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests.StepDefinitions;

[Binding]
public sealed class ServicesNavigationSteps : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly EpamHomePage _homePage;
    private readonly string _baseUrl;

    public ServicesNavigationSteps()
    {
        var settings = TestEnvironmentFixture.Instance.Settings;
        var downloadPath = Path.Combine(Path.GetTempPath(), settings.DownloadPath ?? "downloads");
        Directory.CreateDirectory(downloadPath);

        _driver = BrowserFactory.Create(settings.Browser, downloadPath);
        _driver.Manage().Window.Maximize();
        _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

        _homePage = new EpamHomePage(_driver);
        _baseUrl = settings.BaseUrl;
    }

    private void DismissOneTrustBanner()
    {
        BrowserFactory.DismissOneTrustCookies(_driver);

        var js = (IJavaScriptExecutor)_driver;
        js.ExecuteScript(@"
            const banner = document.getElementById('onetrust-banner-sdk');
            if (banner) { banner.style.display = 'none'; }
            const backdrop = document.getElementById('onetrust-pc-sdk');
            if (backdrop) { backdrop.style.display = 'none'; }
        ");

        var acceptButton = _driver.FindElements(By.Id("onetrust-accept-btn-handler")).FirstOrDefault();
        if (acceptButton is { Displayed: true, Enabled: true })
        {
            try
            {
                acceptButton.Click();
            }
            catch
            {
                // Ignore and continue with cookie/js based dismissal.
            }
        }
    }

    [Given("I am on the EPAM home page")]
    public void GivenIAmOnTheEpamHomePage()
    {
        _driver.Navigate().GoToUrl(_baseUrl);
        DismissOneTrustBanner();
    }

    [When("I hover over the Services menu")]
    public void WhenIHoverOverTheServicesMenu()
    {
        _homePage.HoverServicesMenu();
    }

    [When("I select the \"(.*)\" service category from the dropdown")]
    public void WhenISelectTheServiceCategoryFromTheDropdown(string category)
    {
        DismissOneTrustBanner();
        try
        {
            _homePage.SelectServiceCategory(category);
        }
        catch (ElementClickInterceptedException)
        {
            DismissOneTrustBanner();
            _homePage.SelectServiceCategory(category);
        }
    }

    [Then("the page title should contain \"(.*)\"")]
    public void ThenThePageTitleShouldContain(string expectedText)
    {
        var title = _driver.Title;
        Assert.Contains(expectedText, title, StringComparison.OrdinalIgnoreCase);
    }

    [Then("the \"Our Related Expertise\" section should be displayed")]
    public void ThenTheOurRelatedExpertiseSectionShouldBeDisplayed()
    {
        var section = _driver.FindElements(By.XPath("//*[contains(normalize-space(.), 'Our Related Expertise')]"))
            .FirstOrDefault(el => el.Displayed);

        Assert.NotNull(section);
        Assert.True(section!.Displayed, "The 'Our Related Expertise' section is not visible.");
    }

    public void Dispose()
    {
        try
        {
            _driver?.Quit();
        }
        catch (Exception ex) when (ex is WebDriverException or InvalidOperationException)
        {
            Log.Warning(ex, "Browser cleanup failed during scenario disposal.");
        }
    }
}
