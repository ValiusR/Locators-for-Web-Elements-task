using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Threading;

namespace Locators_for_Web_Elements.Business;

public class InsightsPage : BasePage
{
    public InsightsPage(IWebDriver driver) : base(driver) { }

    public void OpenInsights()
    {
        Wait.Until(d => d.FindElement(By.CssSelector(".top-navigation__item.epam a.top-navigation__item-link[href='/insights']"))).Click();
    }

    public void SwipeCarousel(int times)
    {
        var slider = Wait.Until(d => d.FindElement(By.CssSelector(".slider-ui-23[data-configuration='text-and-image-in-two-columns']")));
        Actions.ScrollToElement(slider).Perform();

        var nextButton = Wait.Until(d => slider.FindElement(By.CssSelector(".slider__right-arrow.slider-navigation-arrow")));

        for (int i = 0; i < times; i++)
        {
            nextButton.Click();
            Thread.Sleep(1000);
        }
    }

    public string GetCurrentArticleTitle()
    {
        var activeSlide = Wait.Until(d =>
        {
            var slides = d.FindElements(By.CssSelector(".slider-ui-23[data-configuration='text-and-image-in-two-columns'] .owl-item.active:not(.cloned)"));
            return slides.Count > 0 ? slides[0] : null;
        });

        return activeSlide.FindElement(By.CssSelector(".single-slide__content .font-size-44")).Text;
    }

    public void ClickReadMore()
    {
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
        return Wait.Until(d => d.FindElement(By.CssSelector(".header_and_download h1"))).Text;
    }
}
