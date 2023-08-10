using Application.Data.Interfaces;
using NLog;

namespace Application.Data.Services;

public class ApplicationService : IApplicationService
{
    private readonly Logger _logger;

    public ApplicationService(Logger logger)
    {
        _logger = logger.Factory.GetLogger("ApplicationService");
    }
    
    public async Task RunService()
    {
        _logger.Debug("Service started");

        await StopService();
    }

    public Task StopService()
    {
        _logger.Debug("Service stopped");

        return Task.CompletedTask;
    }
}