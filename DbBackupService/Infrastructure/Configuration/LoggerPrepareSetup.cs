using NLog;
using NLog.Config;

namespace Infrastructure.Configuration;

public static class LoggerConfiguration
{
    private const string LogFileName = "logs.txt"; 
    public static ISetupBuilder PrepareSetup() =>
        LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().FilterLevel(LogLevel.Debug).WriteToConsole();
            builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToFile(fileName: LogFileName);
        });
}