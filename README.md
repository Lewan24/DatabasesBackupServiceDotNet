# Service Application

## Purpose and project problem
The main problem and reason of creating this kind of application is the frustration of free 3d-party programs that forces user to pay for a functionallity like making databases backups for multiple databases. These programs only works fine for 1 or 2 databases/servers.

So I decided to create own flexible service that will backup databases with no limitations of used connections.
This application is free to use to anyone, it's also open-source so feel free to create issues with new funtionallities or improvements, that I could add. 

The main goal of using my application is just making unlimited backups for databases. 
I personally use it already in my company as simple db backup automation, so I dont need to focus and remember about creating backups.

## Future goals
In near future my goal is to create open-source Docker Application with nice UI that backups your selected dbs.

Everything is gonna be easy and fast to use, simple configuration, easy container update, volumes to store data and configuration on machine to not lost any needed data etc.

## Work logic

### Logic Scheme

![Project scheme](https://github.com/Lewan24/DatabasesBackupServiceDotNet/blob/main/ServiceLogicProject_v1.2_Drawio.png)

### Platform compability
The application was developed firstly for Windows and Linux.

Now the project is realeased to docker hub, so you only need docker on your machine, pull the image, assign volumes and backup your dbs!

### Configuration

Everyting you need to work is in Application, just open on port that you have run the container and set up what you need.

Of course you can just create config files in volume and assing it to container, below are all files that are needed for application to work.
If app detects that files are not compatible or something is missing, then it will change the name of file and will create proper one for your configuration.

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
Visit docker hub here: FutureLink
and follow instructions.

## Email informing functionallity
Inside application there is an additional service that handles sending emails.

You can set all settings in appsettings.json (or after oppening application via browser) like which emails should be sent, who will receive these emails, and of cource email sender credentials to let service use them for sending emails to receivers.

By default the email provider is disabled in settings, so you don't need to enable it or set any options for email provider, the service will not run if the provider is disabled in settings.

### Email send protocol
If you need to send email via StartTls then in settings set parameter "UseStartTls" as true, if UseStartTls is false, then it will check next parameter: "UseSslInstead".
If StartTls and UseSslInstead are false, then email will be set to auto value, so the provider will automatically try to set matching protocol.

## End words
Thanks for visiting my github and repos, I hope you like it and got you interested or even got you inspired to use the idea or application in your projects.

Keep coding. See ya!
