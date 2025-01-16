using Modules.Backup.Infrastructure.DbContexts;
using Modules.Backup.Infrastructure.Interfaces;

namespace Modules.Backup.Infrastructure.Services;

internal sealed class DbScheduleRepo : IDbScheduleRepo
{
    private BackupsDbContext _context = new();

    public async Task SaveChanges()
    {
        await _context.SaveChangesAsync();
    }
}
