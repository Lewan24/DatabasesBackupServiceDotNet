namespace Core.Entities;

public class ApplicationConfiguration
{
    // TODO: Add other configuration properties if needed
    public string? LogsFileName { get; set; } = string.Empty;
    public string? BackupSaveDirectory { get; set; } = string.Empty;
    public bool IncludeDateOfCreateLogFile { get; set; }
}