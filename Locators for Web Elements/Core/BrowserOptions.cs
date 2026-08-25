namespace Locators_for_Web_Elements.Core;

public class BrowserOptions
{
    public bool UserDataDir { get; set; } = true;
    public string Language { get; set; } = "en-US";
    public bool DownloadPrompt { get; set; } = false;
    public bool DirectoryUpgrade { get; set; } = true;
    public bool AlwaysOpenPdfExternally { get; set; } = true;
    public bool Headless { get; set; } = false;
    public bool DisableInfoBars { get; set; } = true;
    public bool DisableDevShmUsage { get; set; } = true;
    public bool NoSandbox { get; set; } = true;
    public bool DisableGpu { get; set; } = true;
}
