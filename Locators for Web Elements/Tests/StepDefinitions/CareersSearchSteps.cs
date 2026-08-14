using Reqnroll;
using Locators_for_Web_Elements.Business;

namespace Locators_for_Web_Elements.Tests.StepDefinitions;

[Binding]
public sealed class CareersSearchSteps
{
    private readonly StepDriverContext _context;
    private readonly EpamHomePage _homePage;
    private readonly CareersSearchPage _careersPage;
    private string _jobPageText = string.Empty;

    public CareersSearchSteps(StepDriverContext context)
    {
        _context = context;
        _homePage = new EpamHomePage(_context.Driver);
        _careersPage = new CareersSearchPage(_context.Driver);
    }

    [Given("I am on the EPAM home page for careers search")]
    public void GivenIAmOnTheEpamHomePageForCareersSearch()
    {
        _context.NavigateToHomePage();
    }

    [When("I search jobs for keyword \"(.*)\" in country \"(.*)\"")]
    public void WhenISearchJobsForKeywordInCountry(string keyword, string country)
    {
        _homePage.ClickCareersLink();
        _careersPage.OpenJobSearchForm();
        _careersPage.SelectCountry(country);
        _careersPage.WaitForPreloaderToDisappear();
        _careersPage.EnterKeyword(keyword);
        _careersPage.EnableFilter("Remote");
        _careersPage.WaitForPreloaderToDisappear();
        _careersPage.SubmitSearch();
    }

    [When("I open the last job card from the search results")]
    public void WhenIOpenTheLastJobCardFromTheSearchResults()
    {
        _careersPage.ClickLastJobCard();
        _jobPageText = _careersPage.GetPageBodyText();
    }

    [Then("the opened job details should contain \"(.*)\"")]
    public void ThenTheOpenedJobDetailsShouldContain(string keyword)
    {
        Assert.Contains(keyword, _jobPageText, StringComparison.OrdinalIgnoreCase);
    }
}
