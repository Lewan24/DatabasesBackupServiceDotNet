using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IDatabase
{
    Task<bool> PerformBackup();
}