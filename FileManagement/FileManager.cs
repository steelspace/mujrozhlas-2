using MujRozhlas.Data;
using MujRozhlas.Downloader;

namespace MujRozhlas.FileManagement;

public static class FileManager
{
    const string downloadFolder = "./episodes";

    static string EnsureSerialFolder(string serialId)
    {
        if (!Directory.Exists(downloadFolder))
        {
            Directory.CreateDirectory(downloadFolder);
        }

        string serialFolder = Path.Combine(downloadFolder, serialId);
        if (!Directory.Exists(serialFolder))
        {
            Directory.CreateDirectory(serialFolder);
        }

        return serialFolder;
    }

    public static string GetFileName(Episode episode)
    {
        string serialFolder = EnsureSerialFolder(episode.SerialId);

        string path = Path.Combine(serialFolder, new SanitizedFileName(episode.Id).Value + ".mp4");        
        return path;
    }
}