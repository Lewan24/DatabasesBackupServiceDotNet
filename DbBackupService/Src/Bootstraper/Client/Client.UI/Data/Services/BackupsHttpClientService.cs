using Modules.Backup.Shared.Dtos;
using OneOf;
using OneOf.Types;

namespace Client.UI.Data.Services;

public class BackupsHttpClientService(TokenHttpClientService api)
{
    public async Task<OneOf<List<ServerConnectionDto>, string>> GetServersAsync()
        => await api.GetAsync<List<ServerConnectionDto>>("/api/servers/GetMyServers");
    
    public async Task<OneOf<List<ServerNameIdDto>, string>> GetServersNamesForSchedulesAsync()
        => await api.GetAsync<List<ServerNameIdDto>>("/api/servers/GetMyServersForSchedule");
    
    public async Task<OneOf<Success, string>> CreateServerAsync(ServerConnectionDto newServer)
        => await api.PostAsync("/api/servers/CreateServer", newServer);
    
    public async Task<OneOf<Success, string>> EditServerAsync(ServerConnectionDto server)
        => await api.PostAsync("/api/servers/EditServer", server);
    
    public async Task<OneOf<Success, string>> ToggleDisabledStatus(Guid serverId)
        => await api.PostAsync("/api/servers/ToggleServerDisabledStatus", serverId);
}