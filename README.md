# Service Application

The main reason of creating this kind of application is the little frustration of free 3d-party programs that forces user to pay for a functionallity of creating database backups for multiple databases and servers. These programs only works fine for 1 max 2 connections and databases.
So I decided to create own flexible service that will backup selected databases.

## Work logic

### Logic Scheme

![Project scheme](https://github.com/Lewan24/DatabasesBackupServiceDotNet/blob/main/ServiceLogicProject_v1.0_Drawio.jpg)

### Configuration

Application needs 2 files to work with:

- appsettings.json
```
{
  "AppConfiguration": {
    "LogsFileName": "Logs.txt",
    "BackupSaveDirectory": "C:\\Backups",
    "IncludeDateOfCreateLogFile": true
  },
  "EmailProviderConfiguration": {
    "EnableEmailProvider": true,
    "SendEmailOnEachDbSuccessfulBackup": false,
    "SendEmailOnEachDbFailureBackup": true,
    "SendEmailWithStatisticsAfterBackups": true,
    "EmailCredentials": {
      "UserEmail": "user@gmail.com",
      "Password": "Passwd",
      "SmtpHost": "smtp.gmail.com",
      "SmtpPort": 587
    }
  }
}
```
- databasesConfigurations.json
```
[
  {
    "DbType": "MySql",
    "DbName": "orders",
    "DbUser": "root",
    "DbPasswd": "root",
    "DbServerAndPort": "localhost:3306"
  },
  {
    "DbType": "PostgreSql",
    "DbName": "TestDb",
    "DbUser": "postgresuser",
    "DbPasswd": "passwd",
    "DbServerAndPort": "localhost:5432"
  }
/// you can use as many connections as you want
]
```
You need to remember that the application only accepts 2 types of databases:
- MySql
- PostgreSql
If the type will be other, then service will throw warning in logs and console and will not backup this one database with invalid db type.

Service will load json as list of configs and for every config (database) will backup it to depending on databasename and type directory with current backup date. Example:
```
├── ApplicationDirectory
    ├── ServiceApplication          # executable file
    ├── Logs.txt                    # all logs from service
    └── Src
        ├── ConfigurationFiles
            ├── appsettings.json                # configuration file for application
            └── databasesConfigurations.json    # configuration file for databases
        └── Media
            └── *Some needed content*           # eg. icon file
└── Backups
    ├── TestDb_localhost_3306
        ├── TestDb_08.08.2023_06.00.zip
        └── TestDb_09.08.2023_06.00.zip
    ├── TestDb_example_3306
        └── TestDb_09.08.2023_06.00.zip
    └── TestDb2_localhost_3306
        └── TestDb2_09.08.2023_06.00.zip
```

## Running application
Service is a console application that doesn't need user integration, so everything is doing automatically, logs and information are stored in log file that will be inside application directory after first run.
Main purpose of running service regularly is to set Windows Task Scheduler:
- Create new Task
- In 'General' section the best options to set are Run with highest privileges, the reason is that sometimes application would like to ask for permissions to create directory or remove created file etc.
- In 'Triggers' section click 'New...' and set how often the service will be run. Eg Daily, every 6.00 am, Stop task if it runs longet than 30 minutes, Enabled
- In 'Actions' section click 'New...', select 'Start a program' from list on the top of window, then click 'Browse...' and select application service.

After these actions the task is ready and will trigger every day at 6.00 am in this specific example.
On linux there is no task scheduler, so probably in the future, I will add functionallity to the service that automatically will be running every some selected time. (The problem is that, the service-console will be oppened 24/7 // need to think on it)

## Project references
Project reference is my public repository for background console application.
This one will be working in similar way like with the help of windows task scheduler.
Project: https://github.com/Lewan24/ServiceConsoleAppSample

## End words
This application for now (09.08.2023) is in project architecture designing process. In some days the .NET project will be uploaded probably as the init project or the ready application.
