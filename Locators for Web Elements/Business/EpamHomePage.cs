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

    public void HoverServicesMenu()
    {
        Logger.Information("Hovering over Services menu");

        var servicesLink = Wait.Until(_ =>
            Driver.FindElements(By.CssSelector("a.top-navigation__item-link.js-op"))
                .FirstOrDefault(link => string.Equals(link.Text, "Services", StringComparison.OrdinalIgnoreCase)));

        if (servicesLink == null)
        {
            throw new NoSuchElementException("Services link was not found in the main navigation.");
        }

        Actions.MoveToElement(servicesLink).Perform();

        Wait.Until(_ => Driver.FindElements(By.CssSelector("a.top-navigation__sub-link"))
            .Any(link => link.Displayed && link.Text.Contains("AI", StringComparison.OrdinalIgnoreCase)));
    }

    public void SelectServiceCategory(string category)
    {
        Logger.Information("Selecting service category: {Category}", category);

        var serviceLink = Wait.Until(_ =>
            Driver.FindElements(By.CssSelector("a.top-navigation__sub-link"))
                .FirstOrDefault(link =>
                    string.Equals(link.Text.Trim(), category, StringComparison.OrdinalIgnoreCase)));

        if (serviceLink == null)
        {
            throw new NoSuchElementException($"Service category '{category}' was not found in the Services dropdown.");
        }

        serviceLink.Click();
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
        ((IJavaScriptExecutor)Driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
    }

    public void ClickPolicyPdfLink(string fileName)
    {
        Logger.Information("Clicking policy PDF link: {FileName}", fileName);
        var normalizedName = fileName.ToLowerInvariant();
        var linkLocator = By.XPath($"//footer//a[contains(translate(@href,'ABCDEFGHIJKLMNOPQRSTUVWXYZ','abcdefghijklmnopqrstuvwxyz'),'{normalizedName}')]");
        var pdfLink = Wait.Until(d => d.FindElement(linkLocator));
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'end'});", pdfLink);
        Wait.Until(_ => pdfLink.Displayed && pdfLink.Enabled);
        pdfLink.Click();
    }

    public bool IsRelatedExpertiseSectionVisible()
    {
        Logger.Information("Checking 'Our Related Expertise' section visibility");
        var section = Wait.Until(_ =>
            Driver.FindElements(By.XPath("//*[contains(normalize-space(.), 'Our Related Expertise')]"))
                .FirstOrDefault(el => el.Displayed));

        return section != null;
    }
}
