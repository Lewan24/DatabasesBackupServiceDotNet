using Application.Data.Interfaces;
using Application.Data.Services;
using Infrastructure.Configuration;
using NLog;

var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Src" ,"ConfigurationFiles", "appsettings.json");
var configJson = File.ReadAllText(configFilePath);
var applicationConfiguration = await PrepareApplicationConfiguration.Prepare(configJson);

var logger = LoggerConfiguration.PrepareSetup(applicationConfiguration).GetCurrentClassLogger();

logger.Info("\nPreparing application services...");

IDbBackupService backupService = new DbBackupService(logger, applicationConfiguration!);
IApplicationService application = new ApplicationService(logger, backupService);

logger.Info("Starting application...");
await application.RunService();

logger.Info("Shutting down Application");
LogManager.Shutdown();

// TODO: Add tests project