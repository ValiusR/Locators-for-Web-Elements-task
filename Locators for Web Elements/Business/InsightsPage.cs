using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Locators_for_Web_Elements.Business;

public class InsightsPage : BasePage
{
    public InsightsPage(IWebDriver driver) : base(driver) { }

    public void OpenInsights()
    {
        Logger.Information("Opening Insights page");
        Wait.Until(d => d.FindElement(By.CssSelector(".top-navigation__item.epam a.top-navigation__item-link[href='/insights']"))).Click();
    }

    public void SwipeCarousel(int times)
    {
        Logger.Information("Swiping carousel {Times} times", times);
        var slider = Wait.Until(d => d.FindElement(By.CssSelector(".slider-ui-23[data-configuration='text-and-image-in-two-columns']")));
        Actions.ScrollToElement(slider).Perform();

        var nextButton = Wait.Until(d => slider.FindElement(By.CssSelector(".slider__right-arrow.slider-navigation-arrow")));

        for (int i = 0; i < times; i++)
        {
            nextButton.Click();
        }
    }

    public string GetCurrentArticleTitle()
    {
        Logger.Information("Getting current article title");

        var titleElement = Wait.Until(d =>
            d.FindElement(By.CssSelector(".slider-ui-23[data-configuration='text-and-image-in-two-columns'] .owl-item.active:not(.cloned) .font-size-44"))
        );
        if (titleElement is null)
        {
            throw new NoSuchElementException("Failed to find active carousel article title element.");
        }
        return titleElement.GetAttribute("textContent").Trim();
    }

    public void ClickReadMore()
    {
        Logger.Information("Clicking Read More button");
        var readMoreHref = Wait.Until(d =>
        {
            var activeSlide = d.FindElement(By.CssSelector(".slider-ui-23[data-configuration='text-and-image-in-two-columns'] .owl-item.active:not(.cloned)"));
            var link = activeSlide.FindElement(By.CssSelector(".slider-cta-link"));
            return link.GetAttribute("href");
        });

        Driver.Navigate().GoToUrl(readMoreHref);
    }

    public string GetArticleDetailTitle()
    {
        Logger.Information("Getting article detail title");
        return Wait.Until(d => d.FindElement(By.CssSelector(".header_and_download h1"))).Text;
    }
}
