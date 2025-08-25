namespace Modules.Shared.Common;

public static class DbCommon
{
    private static readonly string DbFolder = Path.Combine("/app", "db");

    public static readonly string DbPath = Path.Join(DbFolder, "BackupsConfiguration.db");

    public static void CreateDbDirectoryIfNotExists()
    {
        if (!Directory.Exists(DbFolder))
            Directory.CreateDirectory(DbFolder);
    }
}