using Application.Data.Interfaces;
using Application.Data.Services;
using Infrastructure.Configuration;
using NLog;

var logger = LoggerConfiguration.PrepareSetup().GetCurrentClassLogger();

logger.Info("Preparing application services...");

IApplicationService application = new ApplicationService(logger);

logger.Info("Starting application...");
await application.RunService();

logger.Info("Shutting down Application");
LogManager.Shutdown();