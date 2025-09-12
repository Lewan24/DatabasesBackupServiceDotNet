using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Backup.Shared.Hubs;

namespace Modules.Backup.Application.Services;

public class NotifyService(
    ILogger<NotifyService> logger,
    IConfiguration config)
{
    public async Task CallServerCreatedEvent(string userName)
        => await CallEvent(nameof(BackupHub.CallServerCreatedEvent), userName);
    
    public async Task CallServerHasChangedEvent(Guid id)
        => await CallEvent(nameof(BackupHub.CallServerChangedEvent), id);
    
    public async Task CallScheduleCreatedEvent(Guid serverId)
        => await CallEvent(nameof(BackupHub.CallScheduleCreatedEvent), serverId);
    
    public async Task CallScheduleHasChangedEvent(Guid id)
        => await CallEvent(nameof(BackupHub.CallScheduleChangedEvent), id);

    public async Task CallBackupCreatedEvent(string userName)
        => await CallEvent(nameof(BackupHub.CallBackupCreatedEvent), userName);
    
    private async Task CallEvent<T>(string eventName, T data)
    {
        logger.LogInformation("Sending event [{EventName}] to BackupHub with data: {@data}", eventName, data);

        var env = config["NotifyServiceSettings:Env"];
        var scheme = config["NotifyServiceSettings:Scheme"];

        string hostDomain = env!.Equals("Production", StringComparison.OrdinalIgnoreCase)
            ? $"{scheme}://{config["NotifyServiceSettings:ProductionHost"]}"
            : $"{scheme}://localhost:{config["NotifyServiceSettings:DevelopmentPort"]}";

        try
        {
            var connection = new HubConnectionBuilder()
                .WithUrl($"{hostDomain}{BackupHubHelper.HubUrl}")
                .Build();

            await connection.StartAsync();

            if (connection.State == HubConnectionState.Connected)
                await connection.InvokeAsync(eventName, data);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error while connecting to the BackupHub");
        }
    }

}