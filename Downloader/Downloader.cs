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
            DownloadEpisode(audioLink.EpisodeId, audioLink.Url);
        }
    }

    void DownloadEpisode(string episodeId, string url)
    {
        if (!Directory.Exists(downloadFolder))
        {
            Directory.CreateDirectory(downloadFolder);
        }

        string path = Path.Combine(downloadFolder, new SanitizedFileName(episodeId).Value + ".mp3");

        string command = $"ffmpeg -i \"{url}\" -bsf:a aac_adtstoasc -vcodec copy -y -c copy -crf 50 -f mp4 \"{path}\"";
        var t = runner.Run(command);
    }
}