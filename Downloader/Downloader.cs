using MujRozhlas.Data;
using MujRozhlas.Database;
using MujRozhlas.FileManagement;
using MujRozhlas.Runner;

namespace MujRozhlas.Downloader;

public class Downloader
{
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
        string path = FileManager.GetFileName(episode);

        if (episode.IsDownloaded && File.Exists(path))
        {
            Console.WriteLine($"Audio for '{episode.ShortTitle}' part {episode.Part} is already donwloaded.");
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