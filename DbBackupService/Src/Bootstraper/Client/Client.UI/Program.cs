using Blazored.LocalStorage;
using Client.UI.Data.Interfaces;
using Client.UI.Data.Services;
using Microsoft.AspNetCore.Components.Authorization;
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

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<AuthStateProvider>());

builder.Logging.AddFilter((category, level) 
    => level >= LogLevel.Warning || 
       category?.Contains("System.Net.Http.HttpClient") != true);

await builder.Build().RunAsync();
