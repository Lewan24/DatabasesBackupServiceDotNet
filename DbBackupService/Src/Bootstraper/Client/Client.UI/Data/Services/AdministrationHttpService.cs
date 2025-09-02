using Modules.Auth.Shared.ActionsRequests;
using Modules.Auth.Shared.Dtos;
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
}