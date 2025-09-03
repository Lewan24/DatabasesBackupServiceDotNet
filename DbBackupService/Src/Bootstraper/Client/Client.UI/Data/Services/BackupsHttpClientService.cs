using Modules.Backup.Shared.Dtos;
using OneOf;

namespace Client.UI.Data.Services;

public class BackupsHttpClientService(TokenHttpClientService api)
{
    public async Task<OneOf<List<ServerConnectionDto>, string>> GetServersAsync()
        => await api.GetAsync<List<ServerConnectionDto>>("/api/servers/GetMyServers");
}