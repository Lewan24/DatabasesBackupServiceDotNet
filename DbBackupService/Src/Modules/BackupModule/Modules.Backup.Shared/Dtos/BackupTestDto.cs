namespace Modules.Backup.Shared.Dtos;

public class BackupTestDto
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime TestedOn { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}