using OpenQA.Selenium;
using Serilog;

namespace Locators_for_Web_Elements.Business;

public class EpamHomePage : BasePage
{
    public EpamHomePage(IWebDriver driver) : base(driver) { }

    public void NavigateTo(string baseUrl)
    {
        Logger.Information("Navigating to: {Url}", baseUrl);
        Driver.Navigate().GoToUrl(baseUrl);
    }

    public void ClickCareersLink()
    {
        Logger.Information("Clicking Careers link");
        Wait.Until(d => d.FindElement(By.LinkText("Careers"))).Click();
    }

    public void OpenGlobalSearch()
    {
        Logger.Information("Opening global search");
        Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'header-search__button')]"))).Click();
    }

    public void EnterSearchKeyword(string keyword)
    {
        Logger.Information("Entering search keyword: {Keyword}", keyword);
        var searchInput = Wait.Until(d => d.FindElement(By.Id("new_form_search")));
        searchInput.Click();
        searchInput.SendKeys(keyword);
    }

    public void ClickSearchButton()
    {
        Logger.Information("Clicking search button");
        Driver.FindElement(By.CssSelector("button.custom-search-button")).Click();
    }

    public IList<IWebElement> GetSearchResultItems()
    {
        Logger.Information("Getting search result items");
        return Wait.Until(d =>
        {
            var items = d.FindElements(By.ClassName("search-results__item"));
            return items.Count > 0 ? items : null;
        })!;
    }

    public bool AllResultsContainKeyword(string keyword)
    {
        Logger.Information("Validating all results contain keyword: {Keyword}", keyword);
        return GetSearchResultItems()
            .All(item => item.Text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    public void ScrollToFooter()
    {
        Logger.Information("Scrolling to footer");
        var footer = Wait.Until(d => d.FindElement(By.TagName("footer")));
        Actions.ScrollToElement(footer).Perform();
    }

    public void ClickPolicyPdfLink(string fileName)
    {
        Logger.Information("Clicking policy PDF link: {FileName}", fileName);
        var pdfLink = Wait.Until(d => d.FindElement(By.CssSelector(".policies-right a[href*='" + fileName + "']")));
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", pdfLink);
        Wait.Until(d => pdfLink.Displayed);
        pdfLink.Click();
    }
}
