using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;

namespace Locators_for_Web_Elements.Core;

public static class TestUtils
{
    public static string SanitizeFileName(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var normalized = value
            .Replace('/', '-')
            .Replace('\\', '-')
            .Replace(':', '-')
            .Replace('*', '-')
            .Replace('?', '-')
            .Replace('"', '-')
            .Replace('<', '-')
            .Replace('>', '-')
            .Replace('|', '-');

        foreach (var invalidChar in invalidChars)
        {
            normalized = normalized.Replace(invalidChar, '-');
        }

        normalized = normalized.Trim();
        normalized = string.IsNullOrWhiteSpace(normalized) ? "unnamed_test" : normalized;

        return normalized;
    }

    public static void TakeScreenshot(IWebDriver driver, ILogger logger, string testName, string testClassName, string artifactsRoot)
    {
        TakeScreenshot(driver, logger, testName, testClassName, null, artifactsRoot);
    }

    public static void TakeScreenshot(IWebDriver driver, ILogger logger, string testName, string testClassName, string? tag, string artifactsRoot)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            var screenshotDir = string.IsNullOrWhiteSpace(tag)
                ? Path.Combine(artifactsRoot, "Screenshots", testClassName)
                : Path.Combine(artifactsRoot, "Screenshots", testClassName, tag);
            Directory.CreateDirectory(screenshotDir);
            var safeTestName = SanitizeFileName(testName);
            var filePath = Path.Combine(screenshotDir, $"{safeTestName}_{timestamp}.png");

            if (driver is ITakesScreenshot screenshotDriver)
            {
                var screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(filePath);
                logger.Information("Screenshot saved: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to capture screenshot for test: {TestName}", testName);
        }
    }

    public static string WaitForFileDownload(IWebDriver driver, ILogger logger, string downloadPath, string partialFileName, int timeoutSeconds = 30)
    {
        logger.Information("Waiting for file download: {FileName}", partialFileName);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var file = Directory.GetFiles(downloadPath)
                .FirstOrDefault(f => Path.GetFileName(f).Contains(partialFileName));
            return file;
        })!;
    }
}
