using Application.Data.Interfaces;
using Application.Data.Services;
using Infrastructure.Configuration;
using NLog;

var configFile = Path.Combine(Directory.GetCurrentDirectory(), "Src" ,"ConfigurationFiles", "appsettings.json");
var configJson = File.ReadAllText(configFile);

var logger = LoggerConfiguration.PrepareSetup(configJson).GetCurrentClassLogger();

logger.Info("Preparing application services...");

IDbBackupService backupService = new DbBackupService(logger);
IApplicationService application = new ApplicationService(logger, backupService, configJson);

logger.Info("Starting application...");
await application.RunService();

logger.Info("Shutting down Application");
LogManager.Shutdown();

// TODO: Add tests project