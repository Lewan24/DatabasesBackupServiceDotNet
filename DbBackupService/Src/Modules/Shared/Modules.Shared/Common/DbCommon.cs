namespace Modules.Shared.Common;

public static class DbCommon
{
    private static readonly string DbFolder = GetDbDirectory();
    
    public static readonly string DbPath = Path.Join(DbFolder, "OctoBackup.db");

    private static string GetDbDirectory()
    {
        if (OperatingSystem.IsWindows())
            return Path.Combine(
                Path.GetDirectoryName(Environment.ProcessPath)!,
                "db");
        
        if (OperatingSystem.IsLinux())
            return "/db";

        throw new PlatformNotSupportedException("Unsupported OS");
    }
    
    public static void CreateDbDirectoryIfNotExists()
    {
        if (!Directory.Exists(DbFolder))
            Directory.CreateDirectory(DbFolder);
    }
}