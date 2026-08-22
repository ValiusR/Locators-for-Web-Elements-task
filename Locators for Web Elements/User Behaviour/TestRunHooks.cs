using Reqnroll;
using Serilog;

namespace Locators_for_Web_Elements.User_Behaviour;

[Binding]
public sealed class TestRunHooks
{
    [AfterTestRun]
    public static void AfterTestRun()
    {
        Log.CloseAndFlush();
    }
}
