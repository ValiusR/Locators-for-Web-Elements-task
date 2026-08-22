using Reqnroll;
using Locators_for_Web_Elements.Business;
using Locators_for_Web_Elements.Core;
using Serilog;

namespace Locators_for_Web_Elements.User_Behaviour;

[Binding]
public sealed class PolicyDownloadSteps
{
    private readonly StepDriverContext _context;
    private readonly EpamHomePage _homePage;
    private readonly ILogger _logger;
    private string _downloadedFilePath = string.Empty;

    public PolicyDownloadSteps(StepDriverContext context)
    {
        _context = context;
        _homePage = new EpamHomePage(_context.Driver);
        _logger = Log.ForContext<PolicyDownloadSteps>();
    }

    [When("I download the policy file containing \"(.*)\"")]
    public void WhenIDownloadThePolicyFileContaining(string partialFileName)
    {
        _homePage.ScrollToFooter();
        _homePage.ClickPolicyPdfLink(partialFileName);
        _downloadedFilePath = TestUtils.WaitForFileDownload(_context.Driver, _logger, _context.DownloadPath, partialFileName);
    }

    [Then("the downloaded file containing \"(.*)\" should exist")]
    public void ThenTheDownloadedFileContainingShouldExist(string partialFileName)
    {
        Assert.True(File.Exists(_downloadedFilePath), $"Downloaded file not found for partial name '{partialFileName}': {_downloadedFilePath}");
    }
}
