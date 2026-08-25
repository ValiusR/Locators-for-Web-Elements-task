using Reqnroll;
using Locators_for_Web_Elements.Business;

namespace Locators_for_Web_Elements.User_Behaviour;

[Binding]
public sealed class GlobalSearchSteps
{
    private readonly StepDriverContext _context;
    private readonly EpamHomePage _homePage;
    private bool _allResultsContainKeyword;

    public GlobalSearchSteps(StepDriverContext context)
    {
        _context = context;
        _homePage = new EpamHomePage(_context.Driver);
    }

    [When("I run a global search for \"(.*)\"")]
    public void WhenIRunAGlobalSearchFor(string keyword)
    {
        _homePage.OpenGlobalSearch();
        _homePage.EnterSearchKeyword(keyword);
        _homePage.ClickSearchButton();
        _allResultsContainKeyword = _homePage.AllResultsContainKeyword(keyword);
    }

    [Then("all global search results should contain \"(.*)\"")]
    public void ThenAllGlobalSearchResultsShouldContain(string keyword)
    {
        Assert.True(_allResultsContainKeyword, $"Not all search results contained the keyword: {keyword}");
    }
}
