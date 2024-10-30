# Service Application

## Purpose and project problem
The main problem and reason of creating this kind of application is the little frustration of free 3d-party programs that forces user to pay for a functionallity like making databases backups for multiple databases. These programs only works fine for 1 max 2 databases.

So I decided to create own flexible service that will backup multiple (actually with no limit) databases, selected in json configuration file. 
This application is free to use to anyone, it's also open-source so feel free to write any advices, or issues with enchantments, what I could add. 

Anyway the main purpose of using service is just making backups for databases. 
In main goal service includes email informing system, backups encryption, history of backups, statistics report.

## Future goals
In near future my goal is to create open-source portfolio application on github pages that will include configuration generator for each public application that I create.<br>

So there will be dedicated page also for this DbBackupService, where you could generate config file using easy switches and buttons.<br>
There also will be place to upload and modify existing config file, read documentation, tips, how to etc.

## Work logic

### Logic Scheme

![Project scheme](https://github.com/Lewan24/DatabasesBackupServiceDotNet/blob/main/ServiceLogicProject_v1.2_Drawio.png)

### Platform compability
The application was developed primarily for Windows, but is also fully compatible with Linux if needed.

### Configuration

Application needs 2 files to work with:

- appsettings.json<br>
// Use "LogsFileName": "Logs\\\Logs.txt" instead of just Logs.txt to store logs inside Logs folder.<br>
```
{
  "AppConfiguration": {
    "LogsFileName": "Logs.txt",
    "BackupSaveDirectory": "C:\\Backups",
    "IncludeDateOfCreateLogFile": true
  },
  "EmailProviderConfiguration": {
    "ProviderSettings": {
      "EnableEmailProvider": false,
      "UseStartTls": false,
      "UseSslInstead": true,
      "SendEmailOnEachDbSuccessfulBackup": false,
      "SendEmailOnEachDbFailureBackup": true,
      "SendEmailWithStatisticsAfterBackups": true,
      "SendEmailOnOtherFailures": true
    },
    "EmailSenderCredentials": {
      "EmailSender": "user@gmail.com",
      "EmailSenderDisplayName": "Backup Service",
      "Password": "Passwd",
      "SmtpHost": "smtp.gmail.com",
      "SmtpPort": 587
    },
    "EmailReceivers": [
      "admin@gmail.com",
      "moderator@gmail.com"
    ]
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

On linux there is no task scheduler, so the best way is just write a simple bash script to run service. And set the script to be run on some period of time regularly.

## Email informing functionallity
Inside application there is an additional service that handles sending emails.

You can set all settings in appsettings.json like which emails should be sent, who will receive these emails, and of cource email sender credentials to let service use them for sending emails to receivers.

By default the email provider is disabled in settings, so you don't need to enable it or set any options for email provider, the service will not run if the provider si disabled in settings.

### Email send protocol
If you need to send email via StartTls then in settings set parameter "UseStartTls" as true, if UseStartTls is false, then it will check next parameter: "UseSslInstead".
If StartTls and UseSslInstead are false, then email will be set to auto value, so the provider will automatically try to set matching protocol.

## Project references
Project reference is my public repository for background console application.
This one will be working in similar way like with the help of windows task scheduler.
Project: https://github.com/Lewan24/ServiceConsoleAppSample

## End words
Thanks for visiting my github and repos, I hope you like it and got you interested or even got you inspired to use the idea or application in your projects.

Keep coding. See ya!
