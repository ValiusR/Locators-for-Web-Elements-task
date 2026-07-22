using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Diagnostics;

namespace Locators_for_Web_Elements
{
    public class EpamTests : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private readonly string baseUrl;

        public EpamTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

            baseUrl = config["BaseUrl"];

            var options = new ChromeOptions();
            var userDataDir = Path.Combine(Path.GetTempPath(), "epam-chrome-profile");
            options.AddArgument($"--user-data-dir={userDataDir}");
            options.AddArgument("--disable-infobars");
            options.AddUserProfilePreference("intl.accept_languages", "en-US");

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            driver.Navigate().GoToUrl(baseUrl);
        }

        public void Dispose()
        {
            driver?.Quit();
        }
        
        [Theory]
        [InlineData("JavaScript", "United States")]
        [InlineData("Java", "Lithuania")]
        public void Task1_ValidatePositionSearch(string keyword, string country)
        {
            // navigate to search page
            wait.Until(d => d.FindElement(By.LinkText("Careers"))).Click();
            wait.Until(d => d.FindElement(By.CssSelector("[data-gtm-category='job_search_redirect'] a.button-body"))).Click();
            DismissOneTrust();

            // select country dropdown
            IWebElement locationInput = wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='Choose your country']")));

            locationInput.Click();

            // send the country name and hit enter to lock in autocomplete choice
            locationInput.SendKeys(country + Keys.Enter);

            // wait for preloader to vanish
            wait.Until(d => {
                var preloaders = d.FindElements(By.CssSelector("[class^='Preloader_fullSize']"));
                return preloaders.Count == 0 || !preloaders[0].Displayed;
            });

            // input keyword (programming language)
            IWebElement keywordInput = driver.FindElement(By.CssSelector("[data-testid='search-input']"));
            keywordInput.Clear();
            keywordInput.SendKeys(keyword);

            wait.Until(d => d.FindElement(By.CssSelector("label[for^='checkbox-vacancy_type-Remote']:not([disabled])"))).Click();

            wait.Until(d => {
                var preloaders = d.FindElements(By.CssSelector("[class^='Preloader_fullSize']"));
                return preloaders.Count == 0 || !preloaders[0].Displayed;
            });

            // submit search
            IWebElement searchButton = driver.FindElement(By.Name("submit_search_box_button"));
            string urlBeforeClick = driver.Url;

            searchButton.Click();
            wait.Until(d => d.Url != urlBeforeClick);

            wait.Until(d => {
                try
                {
                    // I know it's really bad to use classname here considering the css module hashes will
                    // change (cssSelector would be better that search class^='JobCard')
                    // but I didn't find another good place to use ClassName and I want to fulfill all the requirements
                    var jobCards = d.FindElements(By.ClassName("JobCard_panel__gTD7e"));
                    if (jobCards.Count == 0) return false;

                    var jobLink = jobCards.Last().FindElement(By.CssSelector("a[data-testid='job-card-link']"));

                    new Actions(d).MoveToElement(jobLink).Click().Perform();
                    return true;
                }
                catch (Exception)
                {
                    return false; // retry if anything gets intercepted
                }
            });

            wait.Until(d => d.Url != urlBeforeClick);

            // extract and check text
            string finalPageText = wait.Until(d => {
                return d.FindElement(By.TagName("body")).Text;
            });

            Assert.Contains(keyword, finalPageText, StringComparison.OrdinalIgnoreCase);
        }
    
        [Theory]
        [InlineData("BLOCKCHAIN")]
        [InlineData("Cloud")]
        [InlineData("Automation")]
        public void Task2_ValidateGlobalSearch(string searchKeyword)
        {
            DismissOneTrust();

            IWebElement searchIcon = driver.FindElement(By.XPath("//button[contains(@class, 'header-search__button')]"));
            searchIcon.Click();

            IWebElement globalSearchInput = wait.Until(d =>
            {
                return d.FindElement(By.Id("new_form_search"));
            });

            globalSearchInput.Click();

            globalSearchInput.SendKeys(searchKeyword);

            IWebElement findButton = driver.FindElement(By.CssSelector("button.custom-search-button"));
            findButton.Click();

            var resultItems = wait.Until(d => {
                var items = d.FindElements(By.ClassName("search-results__item"));
                return items.Count > 0 ? items : null;
            });

            bool allContainKeyword = resultItems
            .Select(item => item.Text)
            .All(fullCardText => fullCardText.Contains(searchKeyword, StringComparison.OrdinalIgnoreCase));

            var titles = resultItems.Select(link => link.Text).ToList();

            Debug.WriteLine(string.Join(Environment.NewLine, titles));

            Assert.True(allContainKeyword, $"Not all search results contained the keyword: {searchKeyword}");
        }
        /// <summary>
        /// Dismisses the OneTrust cookie consent banner by setting cookies 
        /// </summary>
        private void DismissOneTrust()
        {
            try
            {
                if (driver is ChromeDriver chrome)
                {
                    var cookies = new Dictionary<string, object?>[]
                    {
                        new Dictionary<string, object?>
                        {
                            ["name"] = "OptanonAlertBoxClosed",
                            ["value"] = "true",
                            ["domain"] = ".epam.com",
                            ["path"] = "/"
                        },
                        new Dictionary<string, object?>
                        {
                            ["name"] = "onetrust-consent-sent",
                            ["value"] = "true",
                            ["domain"] = ".epam.com",
                            ["path"] = "/"
                        }
                    };

                    foreach (var cookie in cookies)
                    {
                        chrome.ExecuteCdpCommand("Network.setCookie", cookie);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DismissOneTrust failed: {ex.Message}");
            }
        }

    }
}