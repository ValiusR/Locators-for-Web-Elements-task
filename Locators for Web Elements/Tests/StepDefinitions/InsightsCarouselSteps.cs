using Reqnroll;
using Locators_for_Web_Elements.Business;

namespace Locators_for_Web_Elements.Tests.StepDefinitions;

[Binding]
public sealed class InsightsCarouselSteps
{
    private readonly StepDriverContext _context;
    private readonly InsightsPage _insightsPage;
    private string _selectedCarouselTitle = string.Empty;
    private string _openedArticleTitle = string.Empty;

    public InsightsCarouselSteps(StepDriverContext context)
    {
        _context = context;
        _insightsPage = new InsightsPage(_context.Driver);
    }

    [Given("I am on the EPAM home page for insights carousel")]
    public void GivenIAmOnTheEpamHomePageForInsightsCarousel()
    {
        _context.NavigateToHomePage();
    }

    [When("I open Insights and swipe the featured carousel (.*) times")]
    public void WhenIOpenInsightsAndSwipeTheFeaturedCarouselTimes(int swipeCount)
    {
        _insightsPage.OpenInsights();
        _insightsPage.SwipeCarousel(swipeCount);
        _selectedCarouselTitle = _insightsPage.GetCurrentArticleTitle();
    }

    [When("I open the selected featured article details")]
    public void WhenIOpenTheSelectedFeaturedArticleDetails()
    {
        _insightsPage.ClickReadMore();
        _openedArticleTitle = _insightsPage.GetArticleDetailTitle();
    }

    [Then("the opened article title should match the selected carousel article title")]
    public void ThenTheOpenedArticleTitleShouldMatchTheSelectedCarouselArticleTitle()
    {
        Assert.Equal(_selectedCarouselTitle, _openedArticleTitle);
    }
}
