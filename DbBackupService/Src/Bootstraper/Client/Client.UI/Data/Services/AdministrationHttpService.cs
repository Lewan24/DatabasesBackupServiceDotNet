using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Dtos;
using Modules.Backup.Shared.Dtos;
using Modules.Backup.Shared.Requests;
using OneOf;
using OneOf.Types;

namespace Client.UI.Data.Services;

public class AdministrationHttpService(TokenHttpClientService api)
{
    public async Task<OneOf<List<UserDto>, string>> GetUsersList() 
        => await api.GetAsync<List<UserDto>>("/api/administration/GetUsers");
    
    public async Task<OneOf<Success, string>> ToggleUserBlockade(string userId)
        => await api.PostAsync("/api/administration/ToggleUserBlockade", userId);

    public async Task<OneOf<Success, string>> EditUserAsync(EditUserRequest request)
        => await api.PostAsync("/api/administration/EditUser", request);

    public async Task<OneOf<List<ServersUsersListDto>, string>> FetchServersUsers()
        => await api.GetAsync<List<ServersUsersListDto>>("/api/servers/GetServersUsers");

    public async Task<OneOf<List<string>, string>> GetUsersThatAccessServer(Guid serverId)
        => await api.PostAsync<List<string>, Guid>("/api/servers/GetUsersThatAccessServer", serverId);
    
    public async Task<OneOf<List<string>, string>> GetAllUsersThatDoesNotHaveAccessToServer(Guid serverId)
        => await api.PostAsync<List<string>, Guid>("/api/servers/GetAllUsersThatDoesNotHaveAccessToServer", serverId);
    
    public async Task<OneOf<Success, string>> RemoveUserAccessFromServer(ModifyServerAccessRequest request)
        => await api.PostAsync("/api/servers/RemoveUserAccessFromServer", request);
    
    public async Task<OneOf<Success, string>> GiveUserAccessToServer(ModifyServerAccessRequest request)
        => await api.PostAsync("/api/servers/GiveUserAccessToServer", request);
}