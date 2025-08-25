using Client.UI.Data.Interfaces;
using Modules.Auth.Shared.Static.Setters;

namespace Client.UI.Data.HttpHandlers;

public class HttpTokenAuthHeaderHandler(IAuthHttpClientService authHttpClientApi) : DelegatingHandler
{
    private readonly IUserToken _userTokenHttpClientApi = authHttpClientApi;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        //Console.WriteLine($"Current token: {_userTokenHttpClientApi.UserToken!.Token}");
        HttpClientAuthHeaderSetter.SetToken(ref request, _userTokenHttpClientApi.UserToken!.Token);

        return await base.SendAsync(request, cancellationToken);
    }
}