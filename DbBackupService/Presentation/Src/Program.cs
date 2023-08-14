using Application.Data.Interfaces;
using Application.Data.Services;
using Infrastructure.Configuration;
using NLog;

var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Src" ,"ConfigurationFiles", "appsettings.json");
var configJson = File.ReadAllText(configFilePath);
var applicationSettings = await PrepareApplicationSettings.Prepare(configJson);

var logger = LoggerConfiguration.PrepareSetup(applicationSettings.AppConfiguration).GetCurrentClassLogger();

logger.Info("Preparing application services...");

IEmailProviderService emailProviderService = new EmailProviderService(applicationSettings.EmailProviderConfiguration, logger);
IDbBackupService backupService = new DbBackupService(logger, applicationSettings.AppConfiguration!, emailProviderService);
IApplicationService applicationService = new ApplicationService(logger, backupService, emailProviderService);

logger.Info("Starting application...");
 await applicationService.RunService();

logger.Info("Shutting down Application");
LogManager.Shutdown();