namespace Locators_for_Web_Elements.Core;

public class LoggingSettings
{
    public string MinLevel { get; set; } = "Information";
    public string FilePath { get; set; } = "Logs/epam-taf-{Date}.log";
    public bool ConsoleOutput { get; set; } = true;
    public int RetainedFileCountLimit { get; set; } = 7;
}
