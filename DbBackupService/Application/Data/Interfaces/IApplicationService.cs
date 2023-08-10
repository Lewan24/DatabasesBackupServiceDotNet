namespace Application.Data.Interfaces;

public interface IApplicationService
{
    Task RunService();
    Task StopService();
}