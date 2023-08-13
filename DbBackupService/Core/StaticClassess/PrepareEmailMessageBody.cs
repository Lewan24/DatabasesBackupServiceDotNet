namespace Core.StaticClassess;

public static class PrepareEmailMessageBody
{
    private static string PrepareMessage(string header, string? body) => $"{PrepareHeader(header)}{body}<br><br>{PrepareFooter()}";

    private static string PrepareHeader(string header) => $"<h4>{header} - report of the day: {DateTime.Today:d}</h4>";
    private static string PrepareFooter() => "<i>Thanks for using Backup Service. See you on next mail :)</i>";

    public static string PrepareStatisticsReport(string? body) => PrepareMessage("Backup service statistics", body);
    public static string PrepareErrorReport(string? body) => PrepareMessage("Backup service error", body);
    public static string PrepareDbBackupSuccessReport(string? body) => PrepareMessage("Backup service backup success", body);
    public static string PrepareDbBackupFailureReport(string? body) => PrepareMessage("Backup service backup failure", body);
}