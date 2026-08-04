using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Locators_for_Web_Elements.Tests;

public class EpamTests : Core.BaseTest
{
    private readonly string DownloadPathLocal;

    public EpamTests()
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("config.json")
            .Build();

        DownloadPathLocal = Path.Combine(Path.GetTempPath(), config["DownloadPath"] ?? "epam-downloads");
    }

    [Theory]
    [InlineData("JavaScript", "United States")]
    [InlineData("Java", "Lithuania")]
    public void Task1_ValidatePositionSearch(string keyword, string country)
    {
        ExecuteTest(() =>
        {
            Logger.Information("Starting Task1: ValidatePositionSearch - Keyword: {Keyword}, Country: {Country}", keyword, country);
            var home = new Business.EpamHomePage(Driver);
            var careersPage = new Business.CareersSearchPage(Driver);

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
            Logger.Information("Task1 passed for keyword: {Keyword}, country: {Country}", keyword, country);
        }, $"Task1_ValidatePositionSearch_{keyword}_{country}");
    }

    [Theory]
    [InlineData("BLOCKCHAIN")]
    [InlineData("Cloud")]
    [InlineData("Automation")]
    public void Task2_ValidateGlobalSearch(string searchKeyword)
    {
        ExecuteTest(() =>
        {
            Logger.Information("Starting Task2: ValidateGlobalSearch - Keyword: {Keyword}", searchKeyword);
            var home = new Business.EpamHomePage(Driver);

            home.DismissOneTrust();
            home.OpenGlobalSearch();
            home.EnterSearchKeyword(searchKeyword);
            home.ClickSearchButton();

            bool allContainKeyword = home.AllResultsContainKeyword(searchKeyword);
            Assert.True(allContainKeyword, $"Not all search results contained the keyword: {searchKeyword}");
            Logger.Information("Task2 passed for keyword: {Keyword}", searchKeyword);
        }, $"Task2_ValidateGlobalSearch_{searchKeyword}");
    }

    [Theory]
    [InlineData("Code-Of-Conduct")]
    public void Task3_ValidateFileDownload(string partialFileName)
    {
        ExecuteTest(() =>
        {
            Logger.Information("Starting Task3: ValidateFileDownload - File: {FileName}", partialFileName);
            var home = new Business.EpamHomePage(Driver);

            home.DismissOneTrust();
            home.ScrollToFooter();
            home.ClickPolicyPdfLink(partialFileName);

            string filePath = WaitForFileDownload(partialFileName);
            Assert.True(File.Exists(filePath), $"Downloaded file not found: {filePath}");
            Logger.Information("Task3 passed. File downloaded: {FilePath}", filePath);
        }, $"Task3_ValidateFileDownload_{partialFileName}");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void Task4_ValidateCarouselArticleTitle(int swipeCount)
    {
        ExecuteTest(() =>
        {
            Logger.Information("Starting Task4: ValidateCarouselArticleTitle - Swipes: {SwipeCount}", swipeCount);
            var home = new Business.EpamHomePage(Driver);
            var insights = new Business.InsightsPage(Driver);

            home.DismissOneTrust();
            insights.OpenInsights();
            insights.SwipeCarousel(swipeCount);
            string articleTitle = insights.GetCurrentArticleTitle();
            insights.ClickReadMore();
            string detailTitle = insights.GetArticleDetailTitle();

            Assert.Equal(articleTitle, detailTitle);
            Logger.Information("Task4 passed. Title matches: {Title}", articleTitle);
        }, $"Task4_ValidateCarouselArticleTitle_{swipeCount}");
    }

    private string WaitForFileDownload(string partialFileName, int timeoutSeconds = 30)
    {
        Logger.Information("Waiting for file download: {FileName}", partialFileName);
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var file = Directory.GetFiles(DownloadPathLocal)
                .FirstOrDefault(f => Path.GetFileName(f).Contains(partialFileName));
            return file;
        })!;
    }
}
