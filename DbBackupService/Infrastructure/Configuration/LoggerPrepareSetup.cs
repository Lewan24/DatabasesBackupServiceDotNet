using Core.Entities;
using NLog;
using NLog.Config;

namespace Infrastructure.Configuration;

public static class LoggerConfiguration
{
    public static ISetupBuilder PrepareSetup(ApplicationConfiguration? config)
    {
        var logsFileName = config?.LogsFileName ?? "logs.txt";

        if (config!.IncludeDateOfCreateLogFile)
        {
            var tempName = logsFileName.Split('.');
            logsFileName = $"{tempName[0]}_{DateTime.Today:dd.MM.yy}";
            
            for (var i = 1; i < tempName.Length; i++)
                logsFileName += $".{tempName[i]}";
        }
        
        return LogManager.Setup().LoadConfiguration(builder =>
        {
            builder.ForLogger().FilterLevel(LogLevel.Debug).WriteToConsole();
            builder.ForLogger().FilterMinLevel(LogLevel.Info).WriteToConsole().WriteToFile(fileName: logsFileName);
        });
    }
}