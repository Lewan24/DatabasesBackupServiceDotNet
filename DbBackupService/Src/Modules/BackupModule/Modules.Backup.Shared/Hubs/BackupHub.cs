using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace Modules.Backup.Shared.Hubs;

public class BackupHub : Hub
{
    public async Task CallServerCreatedEvent(string userName)
    {
        await Clients.All.SendAsync(BackupHubHelper.Events.ServerCreatedEvent.ToString(), userName);
    }

    public async Task CallServerChangedEvent(Guid id)
    {
        await Clients.All.SendAsync(BackupHubHelper.Events.ServerHasChangedEvent.ToString(), id);
    }

    public async Task CallScheduleCreatedEvent(Guid serverId)
    {
        await Clients.All.SendAsync(BackupHubHelper.Events.ScheduleCreatedEvent.ToString(), serverId);
    }

    public async Task CallScheduleChangedEvent(Guid id)
    {
        await Clients.All.SendAsync(BackupHubHelper.Events.ScheduleHasChangedEvent.ToString(), id);
    }

    public async Task CallBackupCreatedEvent(string userName)
    {
        await Clients.All.SendAsync(BackupHubHelper.Events.BackupCreatedEvent.ToString(), userName);
    }
}

public static class BackupHubHelper
{
    public enum Events
    {
        ServerCreatedEvent,
        ServerHasChangedEvent,
        ScheduleCreatedEvent,
        ScheduleHasChangedEvent,
        BackupCreatedEvent
    }

    public const string HubUrl = "/backuphub";

    public static HubConnection GetBasicHubConnection(Uri hubAbsoluteUrl)
    {
        return new HubConnectionBuilder()
            .WithUrl(hubAbsoluteUrl)
            .WithAutomaticReconnect()
            .Build();
    }
}