using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Locators_for_Web_Elements
{
    public class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected readonly Actions Actions;

        public BasePage(IWebDriver driver)
        {
            Driver = driver;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            Wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));
            Actions = new Actions(driver);
        }

        public void DismissOneTrust()
        {
            try
            {
                if (Driver is ChromeDriver chrome)
                {
                    var cookieNames = new[] { "OptanonAlertBoxClosed", "onetrust-consent-sent" };
                    foreach (var name in cookieNames)
                    {
                        chrome.ExecuteCdpCommand("Network.setCookie", new Dictionary<string, object?>
                        {
                            ["name"] = name,
                            ["value"] = "true",
                            ["domain"] = ".epam.com",
                            ["path"] = "/"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"DismissOneTrust failed: {ex.Message}");
            }
        }
    }

    public class EpamHomePage : BasePage
    {
        public EpamHomePage(IWebDriver driver) : base(driver) { }

        public void NavigateTo(string? baseUrl)
        {
            Driver.Navigate().GoToUrl(baseUrl!);
        }

        public void ClickCareersLink()
        {
            Wait.Until(d => d.FindElement(By.LinkText("Careers"))).Click();
        }

        public void OpenGlobalSearch()
        {
            Wait.Until(d => d.FindElement(By.XPath("//button[contains(@class, 'header-search__button')]"))).Click();
        }

        public void EnterSearchKeyword(string keyword)
        {
            var searchInput = Wait.Until(d => d.FindElement(By.Id("new_form_search")));
            searchInput.Click();
            searchInput.SendKeys(keyword);
        }

        public void ClickSearchButton()
        {
            Driver.FindElement(By.CssSelector("button.custom-search-button")).Click();
        }

        public IList<IWebElement> GetSearchResultItems()
        {
            return Wait.Until(d =>
            {
                var items = d.FindElements(By.ClassName("search-results__item"));
                return items.Count > 0 ? items : null;
            })!;
        }

        public bool AllResultsContainKeyword(string keyword)
        {
            return GetSearchResultItems()
                .All(item => item.Text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public void ScrollToFooter()
        {
            var footer = Wait.Until(d => d.FindElement(By.TagName("footer")));
            Actions.ScrollToElement(footer).Perform();
        }

        public void ClickPolicyPdfLink(string fileName)
        {
            var pdfLink = Wait.Until(d => d.FindElement(By.CssSelector(".policies-right a[href*='" + fileName + "']")));
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", pdfLink);
            Wait.Until(d => pdfLink.Displayed);
            pdfLink.Click();
        }
    }

    public class CareersSearchPage : BasePage
    {
        public CareersSearchPage(IWebDriver driver) : base(driver) { }

        public void OpenJobSearchForm()
        {
            Wait.Until(d => d.FindElement(By.CssSelector("[data-gtm-category='job_search_redirect'] a.button-body"))).Click();
        }

        public void SelectCountry(string country)
        {
            var locationInput = Wait.Until(d => d.FindElement(By.CssSelector("input[aria-label='Choose your country']")));
            locationInput.Click();
            locationInput.SendKeys(country + Keys.Enter);
        }

        public void WaitForPreloaderToDisappear()
        {
            Wait.Until(d =>
            {
                var preloaders = d.FindElements(By.CssSelector("[class^='Preloader_fullSize']"));
                return preloaders.Count == 0 || !preloaders[0].Displayed;
            });
        }

        public void EnterKeyword(string keyword)
        {
            var keywordInput = Driver.FindElement(By.CssSelector("[data-testid='search-input']"));
            keywordInput.Clear();
            keywordInput.SendKeys(keyword);
        }

        public void EnableFilter(string filterName)
        {
            Wait.Until(d => d.FindElement(By.CssSelector("label[for^='checkbox-vacancy_type-" + filterName + "']:not([disabled])"))).Click();
        }

        public void SubmitSearch()
        {
            var searchButton = Driver.FindElement(By.Name("submit_search_box_button"));
            var urlBefore = Driver.Url;
            searchButton.Click();
            Wait.Until(d => d.Url != urlBefore);
        }

        public void ClickLastJobCard()
        {
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
            return Wait.Until(d => d.FindElement(By.TagName("body")).Text);
        }
    }

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

    public class EpamTests : IDisposable
    {
        private readonly IWebDriver driver;
        private readonly WebDriverWait wait;
        private readonly string? baseUrl;
        private readonly string downloadPath;

        public EpamTests()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();

            baseUrl = config["BaseUrl"];
            downloadPath = Path.Combine(Path.GetTempPath(), "epam-downloads");
            Directory.CreateDirectory(downloadPath);

            var options = new ChromeOptions();
            var userDataDir = Path.Combine(Path.GetTempPath(), "epam-chrome-profile");
            options.AddArgument($"--user-data-dir={userDataDir}");
            options.AddArgument("--disable-infobars");
            options.AddUserProfilePreference("intl.accept_languages", "en-US");

            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.default_directory", downloadPath);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.IgnoreExceptionTypes(typeof(NoSuchElementException), typeof(StaleElementReferenceException));

            new BasePage(driver).DismissOneTrust();
            driver.Navigate().GoToUrl(baseUrl!);
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
            var home = new EpamHomePage(driver);
            var careersPage = new CareersSearchPage(driver);

            home.ClickCareersLink();
            careersPage.OpenJobSearchForm();
            careersPage.DismissOneTrust();
            
            careersPage.SelectCountry(country);
            careersPage.WaitForPreloaderToDisappear();
            careersPage.EnterKeyword(keyword);
            careersPage.EnableFilter("Remote");
            careersPage.WaitForPreloaderToDisappear();
            careersPage.SubmitSearch();
            careersPage.ClickLastJobCard();

            string pageText = careersPage.GetPageBodyText();
            Assert.Contains(keyword, pageText, StringComparison.OrdinalIgnoreCase);
        }

        [Theory]
        [InlineData("BLOCKCHAIN")]
        [InlineData("Cloud")]
        [InlineData("Automation")]
        public void Task2_ValidateGlobalSearch(string searchKeyword)
        {
            var home = new EpamHomePage(driver);

            home.DismissOneTrust();
            home.OpenGlobalSearch();
            home.EnterSearchKeyword(searchKeyword);
            home.ClickSearchButton();

            bool allContainKeyword = home.AllResultsContainKeyword(searchKeyword);
            Assert.True(allContainKeyword, $"Not all search results contained the keyword: {searchKeyword}");
        }      

        [Theory]
        [InlineData("Code-Of-Conduct")]
        public void Task3_ValidateFileDownload(string partialFileName)
        {
            var home = new EpamHomePage(driver);

            home.DismissOneTrust();
            home.ScrollToFooter();
            home.ClickPolicyPdfLink(partialFileName);

            string filePath = WaitForFileDownload(partialFileName);
            Assert.True(File.Exists(filePath), $"Downloaded file not found: {filePath}");
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void Task4_ValidateCarouselArticleTitle(int swipeCount)
        {
            var home = new EpamHomePage(driver);
            var insights = new InsightsPage(driver);

            home.DismissOneTrust();
            insights.OpenInsights();
            insights.SwipeCarousel(swipeCount);
            string articleTitle = insights.GetCurrentArticleTitle();
            insights.ClickReadMore();
            string detailTitle = insights.GetArticleDetailTitle();

            Assert.Equal(articleTitle, detailTitle);
        }
        private string WaitForFileDownload(string partialFileName, int timeoutSeconds = 30)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(d =>
            {
                var file = Directory.GetFiles(downloadPath)
                    .FirstOrDefault(f => Path.GetFileName(f).Contains(partialFileName));
                return file;
            })!;
        }
    }
}
