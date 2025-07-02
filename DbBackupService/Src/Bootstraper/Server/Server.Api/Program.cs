using Blazored.LocalStorage;
using Client.UI;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Modules.Backup.Api;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using Server.Api.Common;
using Server.Api.Hubs;

const string defaultCorsPolicyName = "Default";

var builder = WebApplication.CreateBuilder(args);

IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

builder.Configuration.AddConfiguration(config);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddEndpointsApiExplorer();

if (builder.Environment.IsDevelopment())
    builder.Services.AddOpenApi();

builder.Services.AddBackupModule();

builder.Services.AddLogging(logging =>
{
    logging.AddConfiguration(config.GetSection("Logging"));
     
    var seqConfig = config.GetSection("SeqConfiguration").Get<SeqConfiguration>();

    if (seqConfig is not null && 
        !string.IsNullOrWhiteSpace(seqConfig.ApiKey) &&
        !string.IsNullOrWhiteSpace(seqConfig.Host))
        logging.AddOpenTelemetry(openTelemetryLoggerOptions =>
        {
            openTelemetryLoggerOptions.IncludeScopes = true;
            openTelemetryLoggerOptions.IncludeFormattedMessage = true;

            openTelemetryLoggerOptions.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri($"{seqConfig.Host}/ingest/otlp/v1/logs");
                exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
                exporter.Headers = $"X-Seq-ApiKey={seqConfig.ApiKey}";
            });
        });
            
    if (builder.Environment.IsDevelopment())
    {
        logging.AddConsole();
    }
});

builder.Services.AddCors(opt =>
{
    opt.AddPolicy(defaultCorsPolicyName, policy =>
    {
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyHeader()
                .AllowAnyOrigin()
                .AllowAnyMethod();
        
        if (builder.Environment.IsProduction())
        {
            var allowedOrigins = config.GetValue<string[]>("AllowedOrigins");

            if (allowedOrigins is null || !allowedOrigins.Any())
                throw new ApplicationException("Production Environment requires to provide allowed origins.");
                
            policy.WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            }
        };
        options.SlidingExpiration = true;
        options.LoginPath = "/account/login";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

// TODO: Implement Auth Module
// builder.Services.AddAuthModule()
//     .AddIdentity<AppUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
//     .AddEntityFrameworkStores<AppIdentityDbContext>()
//     .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

builder.Services.Configure<IdentityOptions>(options =>
{
    // Default Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 5;
    options.Password.RequiredUniqueChars = 1;
});

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddSignalR();

builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        ["application/octet-stream"]);
});

var app = builder.Build();

app.UseResponseCompression();

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
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.MapStaticAssets();
        
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();

app.UseRouting();

app.MapRequiredEndpoints();

app.UseCors(defaultCorsPolicyName);

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapBlazorHub(options =>
{
    options.CloseOnAuthenticationExpiration = true;
});

app.MapHub<BackupHub>("/backuphub");

app.Run();