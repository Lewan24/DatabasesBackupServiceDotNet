using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Modules.Backup.Shared.Hubs;

public class BackupHub : Hub
{
    public async Task CallServerCreatedEvent(string userName)
        => await Clients.All.SendAsync(BackupHubHelper.Events.ServerCreatedEvent.ToString(), userName);
    
    public async Task CallServerChangedEvent(Guid id)
        => await Clients.All.SendAsync(BackupHubHelper.Events.ServerHasChangedEvent.ToString(), id);
    
    public async Task CallScheduleCreatedEvent(Guid serverId)
        => await Clients.All.SendAsync(BackupHubHelper.Events.ScheduleCreatedEvent.ToString(), serverId);
    
    public async Task CallScheduleChangedEvent(Guid id)
        => await Clients.All.SendAsync(BackupHubHelper.Events.ScheduleHasChangedEvent.ToString(), id);
}

public static class BackupHubHelper
{
    public const string HubUrl = "/backuphub";

    public enum Events
    {
        ServerCreatedEvent,
        ServerHasChangedEvent,
        ScheduleCreatedEvent,
        ScheduleHasChangedEvent
    }
    
    public static HubConnection GetBasicHubConnection(Uri hubAbsoluteUrl)
        => new HubConnectionBuilder()
            .WithUrl(hubAbsoluteUrl)
            .WithAutomaticReconnect()
            .Build();
}