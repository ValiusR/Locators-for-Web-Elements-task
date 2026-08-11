namespace Locators_for_Web_Elements.Core;

public class TestSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string Browser { get; set; } = "Chrome";
    public string DownloadPath { get; set; } = "downloads";
    public LoggingSettings Logging { get; set; } = new LoggingSettings();
}
