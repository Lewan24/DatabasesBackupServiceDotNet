using Application.Data.Interfaces;
using Core.Entities.Databases;
using Core.Entities.Models;
using Core.Interfaces;
using Core.StaticClasses;
using NLog;

namespace Application.Data.Services;

public class DbBackupService(
    Logger logger,
    ApplicationConfigurationModel appConfig,
    IEmailProviderService emailProviderService)
    : IDbBackupService
{
    private readonly Logger _logger = logger.Factory.GetLogger(nameof(DbBackupService));

    private int _madeBackupsCounter;

    public async Task RunService(List<DatabaseConfigModel> dbConfigurations)
    {
        _logger.Info("{ServiceName} has started", nameof(DbBackupService));
        _logger.Info("Preparing backups for {DbCConfigsCount} databases...", dbConfigurations.Count);

        await PerformBackup(dbConfigurations);

        _logger.Info("{ServiceName} finished its job and stoped.", nameof(DbBackupService));
    }

    public Task<int> GetBackupsCounter()
    {
        return Task.FromResult(_madeBackupsCounter);
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
                    DatabaseType.MySql => new MySqlDatabase(dbConfig, _logger, appConfig),
                    DatabaseType.PostgreSql => new PostgreSqlDatabase(dbConfig, _logger, appConfig),
                    _ => throw new NotSupportedException(
                        $"Selected type of database is not supported: {dbConfig.DbType}")
                };

                databasesList.Add(database);
            }

            foreach (var db in databasesList)
                try
                {
                    await db.PerformBackup();

                    _madeBackupsCounter++;

                    if ((await emailProviderService.GetEmailSettings()).SendEmailOnEachDbSuccessfulBackup)
                        await emailProviderService.PrepareAndSendEmail(new MailModel(
                            $"Successful backup of '{await db.GetDatabaseName()}'",
                            EmailMessageBody.PrepareDbBackupSuccessReport(
                                $"<b><span style='color: green'>Backup for '{await db.GetDatabaseName()}' has been made with success.</span></b><br>Backup finish time: <b>{DateTime.Now:t}</b>")));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Exception thrown while performing backup in database");

                    if ((await emailProviderService.GetEmailSettings()).SendEmailOnEachDbFailureBackup)
                        await emailProviderService.PrepareAndSendEmail(new MailModel(
                            "There was a problem in the backup service",
                            EmailMessageBody.PrepareDbBackupFailureReport(
                                $"<b><span style='color: red'>Error occurs while performing backup</span></b><br><i><b>Error message: </b><span style='color: red'>{e.Message}</span></i>")));
                }
        }
        catch (NotSupportedException e)
        {
            _logger.Warn(e, "Exception thrown while preparing databases");

            if ((await emailProviderService.GetEmailSettings()).SendEmailOnOtherFailures)
                await emailProviderService.PrepareAndSendEmail(new MailModel(
                    "There was a problem in the backup service",
                    EmailMessageBody.PrepareErrorReport(
                        $"<b><span style='color: red'>Error occurs while doing dbType assignment to each database configuration</span></b><br><i><b>Error message: </b><span style='color: red'>{e.Message}</span></i>")));
        }
    }
}