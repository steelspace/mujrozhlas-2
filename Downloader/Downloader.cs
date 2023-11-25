using MujRozhlas.Builder;
using MujRozhlas.Common;
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

            if (download.IsDownloaded && episode == null)
            {
                continue;
            }

            DownloadEpisode(episode, download.Url);
        }
    }

    void DownloadEpisode(Episode episode, string url)
    {
        string episodeFileNameWithPath = FileManager.GetFileName(episode);

        if (episode.IsDownloaded && File.Exists(episodeFileNameWithPath))
        {
            Console.WriteLine($"Audio for '{episode.ShortTitle}' part {episode.Part} is already donwloaded.");
            return;
        }

        string command = $"ffmpeg -i \"{url}\" -bsf:a aac_adtstoasc -vcodec copy -y -c copy -crf 50 -f mp4 \"{episodeFileNameWithPath}\"";
        runner.Run(command);

        string fileName = Path.GetFileName(episodeFileNameWithPath);
        string? episodeDirectory = Path.GetDirectoryName(episodeFileNameWithPath);

        if (episodeDirectory is null)
        {
            throw new ExtractorException("Episode folder not found");
        }

        var titleAuthor = AudioBookBuilder.GetTitleAuthor(episode.ShortTitle);

        string command2 = $"ffmpeg -y -i \"{fileName}\" -metadata title='{titleAuthor.Item1}'"
            + (!string.IsNullOrEmpty(titleAuthor.Item2) ? $" -metadata author='{titleAuthor.Item2}'" : String.Empty)
            + $" -c copy \"_{fileName}\"";
        runner.Run(command2, Path.GetFullPath(episodeDirectory));

        if (!File.Exists(Path.Combine(episodeDirectory, "_" + fileName)))
        {
            throw new ExtractorException($"Tagging file {"_" + fileName} failed");
        }

        File.Move(Path.GetFullPath(Path.Combine(episodeDirectory, "_" + fileName)),
            Path.GetFullPath(Path.Combine(episodeDirectory, fileName)),
            true);

        if (File.Exists(episodeFileNameWithPath))
        {
            database.SetDownloadFinished(episode.Id);
        }
    }
}