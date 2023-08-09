# Service Application

The main reason of creating this kind of application is the little frustration of free 3d-party programs that forces user to pay for a functionallity of creating database backups for multiple databases and servers. These programs only works fine for 1 max 2 connections and databases.
So I decided to create own flexible service that will backup selected databases.

## Work logic

### Configuration
Every database connection will be configured in Json config file, something like that:
```
[
  {
    DbType: "MySql",
    DbName: "TestDb",
    DbUser: "root",
    DbPasswd: "passwd",
    DbServerAndPort: "localhost:3306"
  },
  {
    DbType: "MySql",
    DbName: "TestDb2",
    DbUser: "root",
    DbPasswd: "passwd",
    DbServerAndPort: "localhost:3306"
  },
  {
    DbType: "PostgreSql",
    DbName: "TestDb",
    DbUser: "root",
    DbPasswd: "passwd",
    DbServerAndPort: "localhost:3306"
  }
  // etc.
]
```
Service will load json as list of configs and for every config (database) will backup it to depending on databasename and type directory with current backup date. Example:
```
├── ServiceApplication.exe
└── Backups
    ├── MySql
        ├── TestDb_08_08_2023.zip
        ├── TestDb_09_08_2023.zip
        └── TestDb2_09_08_2023.zip
    └── PostgreSql
        └── TestDb_09_08_2023.zip
```

## Project references
Project reference is my public repository for background console application.
This one will be working in similar way like with the help of windows task scheduler.
Project: https://github.com/Lewan24/ServiceConsoleAppSample

## End words
This application for now (09.08.2023) is in project architecture designing process. In some days the .NET project will be uploaded probably as the init project or the ready application.
