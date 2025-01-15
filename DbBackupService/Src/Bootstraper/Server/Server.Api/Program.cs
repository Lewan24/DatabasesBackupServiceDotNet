using Scalar.AspNetCore;
using Modules.Backup.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddBackupModule();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.WithTitle("Backup Service Api");
        opt.WithTheme(ScalarTheme.BluePlanet);
        opt.WithDefaultHttpClient(ScalarTarget.Http, ScalarClient.Curl);
        opt.Servers = [];
    });
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.MapFallbackToFile("index.html");
app.UseStaticFiles();

app.MapRequiredEndpoints();

app.Run();