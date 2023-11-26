using System.IO.Compression;
using MujRozhlas.Data;
using MujRozhlas.Downloader;

namespace MujRozhlas.FileManagement;

public static class FileManager
{
    const string downloadFolder = "./episodes";
    const string audioBookFolder = "./books";

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

    static string EnsureAudioBooksFolder()
    {
        if (!Directory.Exists(audioBookFolder))
        {
            Directory.CreateDirectory(audioBookFolder);
        }

        return audioBookFolder;
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
    static string ZipFileSuffix = ".zip";

    public static string GetFileName(Episode episode)
    {
        string serialFolder = EnsureSerialFolder(episode.SerialId);

        string path = Path.Combine(serialFolder, new SanitizedFileName(episode.Id).Value + AudioFileSuffix);
        return path;
    }

    public static string GetAudioBookFileName(Serial serial, bool zip = false)
    {
        string serialFolder = EnsureSerialFolder(serial.Id);

        string path = Path.Combine(serialFolder, new SanitizedFileName(serial.Id).Value + "-book"
            + (zip ? ZipFileSuffix : AudioFileSuffix));
        return path;
    }

    public static string GetNiceAudioBookFileName(Serial serial, bool zip = false)
    {
        string serialFolder = EnsureAudioBooksFolder();

        string path = Path.Combine(serialFolder, new SanitizedFileName(serial.Title).Value 
            + (zip ? ZipFileSuffix : AudioFileSuffix));
        return path;
    }

    public static bool IsAudioBookReady(Serial serial)
    {
        string audioBookPath = GetNiceAudioBookFileName(serial);

        return File.Exists(audioBookPath);
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

    public static string DownloadImageToOutputFolder(Serial serial, string fileName)
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

    public static void RenameAudioBook(Serial serial, bool zip = false)
    {
        string currentFileName = GetAudioBookFileName(serial, zip);

        if (File.Exists(currentFileName))
        {
            File.Move(currentFileName, GetNiceAudioBookFileName(serial, zip), true);
        }
    }

    public static void DeleteSerialFiles(string serialId)
    {
        string path = EnsureSerialFolder(serialId);
        Directory.Delete(path, true);
    }

    public static void ZipAudioBook(IEnumerable<string> files, string zipFilePath, string serialName, string coverArtFilePath)
    {
        File.Delete(zipFilePath);
        using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);

        int chapterNumber = 1;
        foreach (var file in files)
        {
            Console.WriteLine($"Adding to ZIP -> {chapterNumber:D2} - {serialName}");
            archive.CreateEntryFromFile(file, $"{chapterNumber:D2} - {serialName}.mp4");
            chapterNumber++;
        }

        archive.CreateEntryFromFile(coverArtFilePath, "cover.jpg");
    }
}