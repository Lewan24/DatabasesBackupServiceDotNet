using System.IO.Compression;

namespace Core.StaticClasses;

public static class CompressBackupFile
{
    /// <summary>
    ///     Performs zip compression on specific file. Method creates zip file with current short date
    /// </summary>
    /// <example>
    ///     File name: test.sql //
    ///     Zip file: test_11.08.2023_.zip //
    ///     File inside zip: test.sql
    /// </example>
    /// <param name="fileBasePath">Base path of directory where the file currently is</param>
    /// <param name="fileName">name of file that we want to compress</param>
    /// <returns>string of zip file name that contains full path</returns>
    public static string Perform(string fileBasePath, string fileName)
    {
        try
        {
            var completeFilePath = Path.Combine(fileBasePath, fileName);
            var zipFileName = Path.Combine(fileBasePath,
                $"{fileName.Split(".sql")[0]}_{DateTime.Today:dd.MM.yy}_{DateTime.Now:H.mm}.zip");

            if (File.Exists(zipFileName))
                throw new Exception("Zip with the same name already exists. Can't perform compression.");

            using (var sourceStream = new FileStream(completeFilePath, FileMode.Open))
            {
                using var zipStream = new FileStream(zipFileName, FileMode.Create);
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);
                var entryName = Path.GetFileName(completeFilePath);
                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);

                using var entryStream = entry.Open();
                sourceStream.CopyTo(entryStream);  
            }

            if (File.Exists(completeFilePath)) 
                File.Delete(completeFilePath);

            return zipFileName;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}