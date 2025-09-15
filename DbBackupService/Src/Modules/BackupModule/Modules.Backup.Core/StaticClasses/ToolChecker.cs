using System.Diagnostics;

namespace Modules.Backup.Core.StaticClasses;

public static class ToolChecker
{
    public static bool IsToolAvailable(string toolName)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = toolName,
                    Arguments = "--version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit(2000);

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}