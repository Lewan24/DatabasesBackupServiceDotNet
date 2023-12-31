using Application.Data.Interfaces;
using Core.Entities.Databases;
using Core.Entities.Models;
using Core.Interfaces;
using Core.StaticClassess;
using NLog;

namespace Application.Data.Services;

public class DbBackupService : IDbBackupService
{
    private readonly Logger _logger;
    private readonly ApplicationConfigurationModel _appConfig;
    private readonly IEmailProviderService _emailProviderService;

    private int _madeBackupsCounter;

    public DbBackupService(Logger logger, ApplicationConfigurationModel appConfig, IEmailProviderService emailProviderService)
    {
        _appConfig = appConfig;
        _emailProviderService = emailProviderService;
        _logger = logger.Factory.GetLogger(nameof(DbBackupService));
    }
    
    public async Task RunService(List<DatabaseConfigModel> dbConfigurations)
    {
        _logger.Info("{ServiceName} has started", nameof(DbBackupService));
        _logger.Info("Preparing backups for {DbCConfigsCount} databases...", dbConfigurations.Count);
        
        await PerformBackup(dbConfigurations);
        
        _logger.Info("{ServiceName} finished its job and stoped.", nameof(DbBackupService));
    }

    private async Task PerformBackup(List<DatabaseConfigModel> dbConfigurations)
    {
        try
        {
            var databasesList = new List<IDatabase>();

            foreach (var dbConfig in dbConfigurations)
            {
                IDatabase database = dbConfig.DbType switch
                {
                    DatabaseType.MySql => new MySqlDatabase(dbConfig, _logger, _appConfig),
                    DatabaseType.PostgreSql => new PostgreSqlDatabase(dbConfig, _logger, _appConfig),
                    _ => throw new NotSupportedException(
                        $"Selected type of database is not supported: {dbConfig.DbType}")
                };

                databasesList.Add(database);
            }

            foreach (var db in databasesList)
            {
                try
                {
                    await db.PerformBackup();

                    _madeBackupsCounter++;

                    if ((await _emailProviderService.GetEmailSettings()).SendEmailOnEachDbSuccessfulBackup)
                        await _emailProviderService.PrepareAndSendEmail(new MailModel($"Successful backup of '{await db.GetDatabaseName()}'",
                            PrepareEmailMessageBody.PrepareDbBackupSuccessReport($"<b><span style='color: green'>Backup for '{await db.GetDatabaseName()}' has been made with success.</span></b><br>Backup finish time: <b>{DateTime.Now:t}</b>")));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Exception thrown while performing backup in database");

                    if ((await _emailProviderService.GetEmailSettings()).SendEmailOnEachDbFailureBackup)
                        await _emailProviderService.PrepareAndSendEmail(new MailModel("There was a problem in the backup service",
                            PrepareEmailMessageBody.PrepareDbBackupFailureReport($"<b><span style='color: red'>Error occurs while performing backup</span></b><br><i><b>Error message: </b><span style='color: red'>{e.Message}</span></i>")));
                }
            }
        }
        catch (NotSupportedException e)
        {
            _logger.Warn(e, "Exception thrown while preparing databases");

            if ((await _emailProviderService.GetEmailSettings()).SendEmailOnOtherFailures)
                await _emailProviderService.PrepareAndSendEmail(new MailModel("There was a problem in the backup service",
                    PrepareEmailMessageBody.PrepareErrorReport($"<b><span style='color: red'>Error occurs while doing dbType assignment to each database configuration</span></b><br><i><b>Error message: </b><span style='color: red'>{e.Message}</span></i>")));
        }
    }
    
    public Task<int> GetBackupsCounter() => Task.FromResult(_madeBackupsCounter);
}