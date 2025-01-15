namespace Modules.Backup.Infrasctructure.Interfaces;

public interface IDbScheduleRepo
{
    Task SaveChanges();
}
