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
        var audioLinks = database.GetAllAudioLinks();

        if (audioLinks is null)
        {
            Console.WriteLine("No audiolinks in queue.");
            return;
        }

        foreach (var audioLink in audioLinks)
        {
            var episode = database.GetEpisode(audioLink.EpisodeId);
            DownloadEpisode(episode, audioLink.Url);
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

        string command = $"ffmpeg -i \"{url}\" -bsf:a aac_adtstoasc -vcodec copy -y -c copy -crf 50 -f mp4 \"{path}\"";
        runner.Run(command);

        if (File.Exists(path))
        {
            database.SetDownloadFinished(episode.Id);
        }
    }
}