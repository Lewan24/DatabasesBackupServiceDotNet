using Microsoft.AspNetCore.SignalR;

namespace Server.Api.Hubs;

public class BackupHub : Hub
{
    public async Task CallTestEvent()
    {
        await Clients.All.SendAsync("OnTestEvent");
    }
}