namespace Core.Entities.Models;

public class ApplicationConfigurationModel
{
    public string? LogsFileName { get; set; } = string.Empty;
    public string? BackupSaveDirectory { get; set; } = string.Empty;
    public bool IncludeDateOfCreateLogFile { get; set; } = false;
    public int TimeInDaysToHoldBackups { get; set; } = 2;
}