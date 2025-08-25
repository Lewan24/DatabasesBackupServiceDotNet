using Client.UI.Data.Interfaces;
using Modules.Auth.Shared.Static.Setters;

namespace Client.UI.Data.HttpHandlers;

public class HttpTokenAuthHeaderHandler(IAuthService authApi) : DelegatingHandler
{
    private readonly IUserToken _userTokenApi = authApi;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        //Console.WriteLine($"Current token: {_userTokenApi.UserToken!.Token}");
        HttpClientAuthHeaderSetter.SetToken(ref request, _userTokenApi.UserToken!.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}