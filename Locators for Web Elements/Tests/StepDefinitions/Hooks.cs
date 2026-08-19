using OpenQA.Selenium;
using Reqnroll;
using Serilog;
using Locators_for_Web_Elements.Core;

namespace Locators_for_Web_Elements.Tests.StepDefinitions;

[Binding]
public sealed class TestHooks
{
    private readonly StepDriverContext _context;
    private readonly ILogger _logger;

    public TestHooks(StepDriverContext context)
    {
        _context = context;
        _logger = Log.ForContext<TestHooks>();
    }

    [BeforeScenario]
    public void BeforeScenario(ScenarioContext scenarioContext)
    {
        _context.Logger.Information("Scenario starting: {ScenarioTitle}", scenarioContext.ScenarioInfo.Title);
        BrowserFactory.DismissOneTrustCookies(_context.Driver);
    }

    [AfterScenario]
    public void AfterScenario(ScenarioContext scenarioContext)
    {
        try
        {
            var testFailed = scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.TestError;
            var scenarioTitle = scenarioContext.ScenarioInfo.Title;

            if (testFailed)
            {
                _logger.Error("Scenario '{ScenarioTitle}' failed", scenarioTitle);
                
                TestUtils.TakeScreenshot(
                    _context.Driver,
                    _logger,
                    scenarioTitle,
                    GetType().Assembly.GetName().Name ?? "Tests"
                );
            }
            else
            {
                _logger.Information("Scenario '{ScenarioTitle}' passed", scenarioTitle);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error during scenario teardown");
        }
    }
}
