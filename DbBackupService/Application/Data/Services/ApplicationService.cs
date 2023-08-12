using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Application.Data.Interfaces;
using Core.Entities.Models;
using Core.Exceptions;
using Newtonsoft.Json;
using NLog;

namespace Application.Data.Services;

public class ApplicationService : IApplicationService
{
    private readonly Logger _logger;
    private readonly IDbBackupService _backupService;
    
    public ApplicationService(Logger logger, IDbBackupService backupService)
    {
        _logger = logger.Factory.GetLogger(nameof(ApplicationService));
        _backupService = backupService;
    }
    
    public async Task RunService()
    {
        _logger.Debug("Service started");

        try
        {
            _logger.Info("Getting needed configurations...");
            var dbConfigurationsList = await ReadDatabaseConfigurations();

            if (dbConfigurationsList.Count == 0)
                throw new EmptyListOfConfigurationsException("Can't receive any db configuration");

            await _backupService.RunService(dbConfigurationsList);
        }
        catch (EmptyListOfConfigurationsException e)
        {
            _logger.Warn(e);
        }
        finally
        {
            await StopService();
        }
    }

    private Task<List<DatabaseConfigModel>> ReadDatabaseConfigurations()
    {
        try
        {
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Src", "ConfigurationFiles", "databasesConfigurations.json");
            var jsonContent = File.ReadAllText(jsonFilePath);
            
            var configs = JsonConvert.DeserializeObject<List<DatabaseConfigModel>>(jsonContent)!;
            
            return Task.FromResult(configs);
        }
        catch (Exception e)
        {
            _logger.Warn(e, "Error on reading config file");
            return Task.FromResult(new List<DatabaseConfigModel>());
        }
    }
    
    private async Task StopService()
    {
        var madeBackupsCount = await _backupService.GetBackupsCounter();
        _logger.Info("{ServiceName} successfully stopped with {BackupsCount} made backups", nameof(ApplicationService), madeBackupsCount);
        
        _logger.Debug("Service stopped");
    }
}