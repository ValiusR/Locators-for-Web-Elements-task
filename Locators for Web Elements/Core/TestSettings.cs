namespace Locators_for_Web_Elements.Core;

public class TestSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string ApiBaseUrl { get; set; } = "https://jsonplaceholder.typicode.com";
    public int ApiTimeoutMs { get; set; } = 30000;
    public string Browser { get; set; } = "Chrome";
    public string DownloadPath { get; set; } = "downloads";
    public LoggingSettings Logging { get; set; } = new LoggingSettings();
}
