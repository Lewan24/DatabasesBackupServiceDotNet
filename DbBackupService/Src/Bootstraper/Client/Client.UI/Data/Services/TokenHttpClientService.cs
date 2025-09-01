using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OneOf;
using OneOf.Types;

namespace Client.UI.Data.Services;

public sealed class TokenHttpClientService(IHttpClientFactory factory)
{
    private readonly HttpClient _http = factory.CreateClient("token");

    public async Task<OneOf<T, string>> GetAsync<T>(string url)
        => await SendRequestAsync<T>(() => _http.GetAsync(url));

    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        try
        {
            return await _http.GetAsync(url);
        }
        catch
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }

    public async Task<OneOf<Success, string>> PostAsync<T>(string url, T data)
        => await SendRequestAsync(() => _http.PostAsJsonAsync(url, data));

    public async Task<OneOf<T, string>> PostAsync<T, TU>(string url, TU data)
        => await SendRequestAsync<T>(() => _http.PostAsJsonAsync(url, data));


    private async Task<OneOf<T, string>> SendRequestAsync<T>(Func<Task<HttpResponseMessage>> httpCall)
    {
        try
        {
            var response = await httpCall();

            if (response.StatusCode is HttpStatusCode.BadRequest 
                                    or HttpStatusCode.NotFound 
                                    or HttpStatusCode.Forbidden)
                return await response.Content.ReadAsStringAsync();

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return "Dostęp zabroniony. Skontaktuj się z Administratorem";

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<T>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result!;
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    private async Task<OneOf<Success, string>> SendRequestAsync(Func<Task<HttpResponseMessage>> httpCall)
    {
        try
        {
            var response = await httpCall();

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            return new Success();
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }
}