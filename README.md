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
  "LogsFileName": "Logs.txt",
  "BackupSaveDirectory": "/home/user/Desktop/DbBackups",
  "IncludeDateOfCreateLogFile": true
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
]
```
Service will load json as list of configs and for every config (database) will backup it to depending on databasename and type directory with current backup date. Example:
```
├── Application
    ├── ServiceApplication          # executable file
    └── Src
        ├── ConfigurationFiles
└── Backups
    ├── TestDb_localhost:3306
        ├── TestDb_08_08_2023_06:00.zip
        └── TestDb_09_08_2023_06:00.zip
    ├── TestDb_example:3306
        └── TestDb_09_08_2023_06:00.zip
    └── TestDb2_localhost:3306
        └── TestDb2_09_08_2023_06:00.zip
```
The naming of backups is to think about.

## Project references
Project reference is my public repository for background console application.
This one will be working in similar way like with the help of windows task scheduler.
Project: https://github.com/Lewan24/ServiceConsoleAppSample

## End words
This application for now (09.08.2023) is in project architecture designing process. In some days the .NET project will be uploaded probably as the init project or the ready application.
