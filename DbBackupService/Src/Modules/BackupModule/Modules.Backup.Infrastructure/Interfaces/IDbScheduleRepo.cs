namespace Modules.Backup.Infrastructure.Interfaces;

public interface IDbScheduleRepo
{
    Task SaveChanges();
}
