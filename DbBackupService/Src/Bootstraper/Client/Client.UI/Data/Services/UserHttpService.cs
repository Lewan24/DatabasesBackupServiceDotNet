using OneOf;

namespace Client.UI.Data.Services;

public class UserHttpService(TokenHttpClientService api)
{
    public async Task<OneOf<bool, string>> AmIAdmin() 
        => await api.GetAsync<bool>("/api/administration/AmIAdmin");
}