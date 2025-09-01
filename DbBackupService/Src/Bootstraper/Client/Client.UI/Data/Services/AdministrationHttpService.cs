using OneOf;

namespace Client.UI.Data.Services;

public class AdministrationHttpService(TokenHttpClientService api)
{
    public async Task<OneOf<bool, string>> IsUserAdmin(string username) 
        => await api.PostAsync<bool, string>("/api/administration/IsUserAdmin", username);
}