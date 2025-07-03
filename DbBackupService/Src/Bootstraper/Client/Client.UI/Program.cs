using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddMudServices();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

builder.Services.AddOptions();
builder.Services.AddAuthorizationCore();

builder.Logging.AddFilter((category, level) 
    => level >= LogLevel.Warning || 
       category?.Contains("System.Net.Http.HttpClient") != true);

await builder.Build().RunAsync();
