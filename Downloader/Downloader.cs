using Mujrozhlas.Data;
using Mujrozhlas.Database;
using Mujrozhlas.Runner;

namespace Mujrozhlas.Downloader;

public class Downloader
{
    const string downloadFolder = "./episodes";
    private readonly IRunner runner;
    private readonly IDatabase database;

    public Downloader(IDatabase database, IRunner runner)
    {
        this.runner = runner;
        this.database = database;
    }

    public void DownloadAllAudioLinks()
    {
        var downloads = database.GetAllDownloads();

        if (downloads.Count == 0)
        {
            Console.WriteLine("No download in queue.");
            return;
        }

        foreach (var download in downloads)
        {
            var episode = database.GetEpisode(download.Id);

            DownloadEpisode(episode, download.Url);
        }
    }

    void DownloadEpisode(Episode episode, string url)
    {
        if (!Directory.Exists(downloadFolder))
        {
            Directory.CreateDirectory(downloadFolder);
        }

        var serialId = episode.SerialId!;
        string serialFolder = Path.Combine(downloadFolder, serialId);
        if (!Directory.Exists(serialFolder))
        {
            Directory.CreateDirectory(serialFolder);
        }

        string path = Path.Combine(serialFolder, new SanitizedFileName(episode.Id).Value + ".mp3");

        if (episode.IsDownloaded && File.Exists(path))
        {
            Console.WriteLine($"Link {url} for '{episode.ShortTitle}' part {episode.Part} is already donwloaded.");
            return;
        }

        string command = $"ffmpeg -i \"{url}\" -bsf:a aac_adtstoasc -vcodec copy -y -c copy -crf 50 -f mp4 \"{path}\"";
        runner.Run(command);

        if (File.Exists(path))
        {
            database.SetDownloadFinished(episode.Id);
        }
    }
}