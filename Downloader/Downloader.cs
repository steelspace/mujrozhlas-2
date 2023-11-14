using Mujrozhlas.Data;

namespace Mujrozhlas.Downloader;

public class Downloader
{
    const string downloadFolder = "episodes";

    void DownloadEpisode(Episode episode)
    {
        string path = Path.Combine(downloadFolder, new SanitizedFileName(episode.Id).Value, ".mp3");
        
    }

    void DownloadM3u8Stream(string url)
    {

    }
}