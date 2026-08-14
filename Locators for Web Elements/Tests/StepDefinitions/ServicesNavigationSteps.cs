using OpenQA.Selenium;
using Reqnroll;
using Locators_for_Web_Elements.Business;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests.StepDefinitions;

[Binding]
public sealed class ServicesNavigationSteps
{
    private readonly StepDriverContext _context;
    private readonly EpamHomePage _homePage;

    public ServicesNavigationSteps(StepDriverContext context)
    {
        _context = context;
        _homePage = new EpamHomePage(_context.Driver);
    }

    [Given("I am on the EPAM home page")]
    public void GivenIAmOnTheEpamHomePage()
    {
        _context.NavigateToHomePage();
    }

    [When("I hover over the Services menu")]
    public void WhenIHoverOverTheServicesMenu()
    {
        _homePage.HoverServicesMenu();
    }

    [When("I select the \"(.*)\" service category from the dropdown")]
    public void WhenISelectTheServiceCategoryFromTheDropdown(string category)
    {
        BrowserFactory.DismissOneTrustCookies(_context.Driver);
        try
        {
            _homePage.SelectServiceCategory(category);
        }
        catch (ElementClickInterceptedException)
        {
            BrowserFactory.DismissOneTrustCookies(_context.Driver);
            _homePage.SelectServiceCategory(category);
        }
    }

    [Then("the page title should contain \"(.*)\"")]
    public void ThenThePageTitleShouldContain(string expectedText)
    {
        var title = _context.Driver.Title;
        Assert.Contains(expectedText, title, StringComparison.OrdinalIgnoreCase);
    }

    [Then("the \"Our Related Expertise\" section should be displayed")]
    public void ThenTheOurRelatedExpertiseSectionShouldBeDisplayed()
    {
        var section = _context.Driver.FindElements(By.XPath("//*[contains(normalize-space(.), 'Our Related Expertise')]"))
            .FirstOrDefault(el => el.Displayed);

        Assert.NotNull(section);
        Assert.True(section!.Displayed, "The 'Our Related Expertise' section is not visible.");
    }
}
