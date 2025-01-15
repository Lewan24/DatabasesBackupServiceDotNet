using Modules.Backup.Infrasctructure.DbContexts;
using Modules.Backup.Infrasctructure.Interfaces;

namespace Modules.Backup.Infrasctructure.Services;

internal sealed class DbScheduleRepo : IDbScheduleRepo
{
    private DbBackupScheduleContext _context = new();

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
