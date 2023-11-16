using MujRozhlas.Data;
using MujRozhlas.Downloader;

namespace MujRozhlas.FileManagement;

public static class FileManager
{
    const string downloadFolder = "./episodes";

    static string EnsureFolder(string rootFolder, string subFolder)
    {
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }

        string folder = Path.Combine(rootFolder, subFolder);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    static string EnsureSerialFolder(string serialId)
    {
        return EnsureFolder(downloadFolder, serialId);
    }

    static string EnsureOutputFolder(string serialId)
    {
        return EnsureFolder(downloadFolder, serialId);
    }

    static string AudioFileSuffix = ".mp4";

    public static string GetFileName(Episode episode)
    {
        string serialFolder = EnsureSerialFolder(episode.SerialId);

        string path = Path.Combine(serialFolder, new SanitizedFileName(episode.Id).Value + AudioFileSuffix);        
        return path;
    }

    public static string GetAudioBookFileName(Serial serial)
    {
        string serialFolder = EnsureSerialFolder(serial.Id);

        string path = Path.Combine(serialFolder, new SanitizedFileName(serial.Id).Value + "-book" + AudioFileSuffix);        
        return path;
    }

    public static string GetNiceAudioBookFileName(Serial serial)
    {
        string serialFolder = EnsureSerialFolder(serial.Id);

        string path = Path.Combine(serialFolder, new SanitizedFileName(serial.Title).Value + AudioFileSuffix);        
        return path;
    }

    public static bool IsEpisodeDownloaded(Episode episode)
    {
        string fileName = GetFileName(episode);
        return File.Exists(fileName);
    }

    public static string WriteBuilderTextFile(Serial serial, string fileName, string content)
    {
        string path = EnsureOutputFolder(serial.Id);
        string filePath = Path.Combine(path, fileName);

        File.WriteAllText(filePath, content);

        return filePath;
    }

    public static string GetFullPathToSerialFolder(Serial serial)
    {
        return Path.GetFullPath(Path.Combine(downloadFolder, new SanitizedFileName(serial.Id).Value));
    }

    public static string DownloadImageToOutputFilder(Serial serial, string fileName)
    {
        if (string.IsNullOrEmpty(serial.CoverArtUrl))
        {
            Console.WriteLine("Cover image URL is empty.");
            return String.Empty;
        }

        string path = Path.Combine(EnsureOutputFolder(serial.Id), fileName);

        using (var client = new HttpClient())
        {
            var imageBytes = client.GetByteArrayAsync(serial.CoverArtUrl).Result;
            File.WriteAllBytes(path, imageBytes);
        }

        return path;
    }

    public static void RenameAudioBook(Serial serial)
    {
        string currentFileName = GetAudioBookFileName(serial);

        if (File.Exists(currentFileName))
        {
            File.Move(currentFileName, GetNiceAudioBookFileName(serial), true);
        }
    }
}