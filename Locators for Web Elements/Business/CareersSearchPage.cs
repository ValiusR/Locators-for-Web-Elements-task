using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Locators_for_Web_Elements.Business;

public class CareersSearchPage : BasePage
{
    public CareersSearchPage(IWebDriver driver) : base(driver) { }

    public void OpenJobSearchForm()
    {
        Logger.Information("Opening job search form");
        Wait.Until(d => d.FindElement(By.CssSelector("[data-gtm-category='job_search_redirect'] a.button-body"))).Click();
    }

    public void SelectCountry(string country)
    {
        Logger.Information("Selecting country: {Country}", country);
        var locationInput = Wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='Choose your country']")));
        locationInput.Click();
        locationInput.SendKeys(country + Keys.Enter);
    }

    public void WaitForPreloaderToDisappear()
    {
        Logger.Information("Waiting for preloader to disappear");
        Wait.Until(d =>
        {
            var preloaders = d.FindElements(By.CssSelector("[class^='Preloader_fullSize']"));
            return preloaders.Count == 0 || !preloaders[0].Displayed;
        });
    }

    public void EnterKeyword(string keyword)
    {
        Logger.Information("Entering keyword: {Keyword}", keyword);
        var keywordInput = Driver.FindElement(By.CssSelector("[data-testid='search-input']"));
        keywordInput.Clear();
        keywordInput.SendKeys(keyword);
    }

    public void EnableFilter(string filterName)
    {
        Logger.Information("Enabling filter: {FilterName}", filterName);
        Wait.Until(d => d.FindElement(By.CssSelector("label[for^='checkbox-vacancy_type-" + filterName + "']:not([disabled])"))).Click();
    }

    public void SubmitSearch()
    {
        Logger.Information("Submitting search");
        var searchButton = Driver.FindElement(By.Name("submit_search_box_button"));
        var urlBefore = Driver.Url;
        searchButton.Click();
        Wait.Until(d => d.Url != urlBefore);
    }

    public void ClickLastJobCard()
    {
        Logger.Information("Clicking last job card");
        var urlBefore = Driver.Url;
        Wait.Until(d =>
        {
            try
            {
                var jobCards = d.FindElements(By.CssSelector("[data-testid='accordion-section-container']"));
                if (jobCards.Count == 0) return false;

                var jobLink = jobCards.Last().FindElement(By.CssSelector("a[data-testid='job-card-link']"));
                Actions.ScrollToElement(jobLink).MoveToElement(jobLink).Click().Perform();
                return true;
            }
            catch
            {
                return false;
            }
        });
        Wait.Until(d => d.Url != urlBefore);
    }

    public string GetPageBodyText()
    {
        Logger.Information("Getting page body text");
        return Wait.Until(d => d.FindElement(By.TagName("body")).Text);
    }
}
