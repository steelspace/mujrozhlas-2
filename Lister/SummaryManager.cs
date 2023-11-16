using BetterConsoleTables;
using CommunityToolkit.Common;
using MujRozhlas.Database;
using MujRozhlas.FileManagement;


namespace MujRozhlas.Lister;

public class SummaryManager
{
    private readonly IDatabase database;

    public SummaryManager(IDatabase database)
    {
        this.database = database;
    }

    public void ListSerials()
    {
        var requestedSerials = database.GetAllSerials();

        if (requestedSerials.Count == 0)
        {
            Console.WriteLine("No serials are requested.");
            return;
        }

        var table = new Table("Serial", "Id", "Total Parts", "Downloaded", "Available", "Unavailable", "Missing")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var serial in requestedSerials)
        {
            var episodes = database.GetEpisodes(serial.Id);
            var downloaded = episodes.Where(FileManager.IsEpisodeDownloaded);
            var availableAudioLinks = episodes.Where(e => e.AudioLinks.Any(al => al.PlayableTill > DateTimeOffset.Now)).ToArray();
            var unavailableAudioLinks = episodes.Where(e => e.AudioLinks.Any(al => al.PlayableTill < DateTimeOffset.Now)).ToArray();

            bool hasUndownloaded = 
                downloaded.MaxOrDefault(e => e.Part) < availableAudioLinks.MaxOrDefault(e => e.Part);

            table.AddRow(serial.Title.Truncate(30, true), serial.Id, serial.TotalParts,
                    $"{downloaded.MinDefault(al => al.Part)}-{downloaded.MaxOrDefault(al => al.Part)}",
                    $"{availableAudioLinks.MinDefault(al => al.Part)}-{availableAudioLinks.MaxOrDefault(al => al.Part)}",
                    $"{unavailableAudioLinks.MinDefault(al => al.Part)}-{unavailableAudioLinks.MaxOrDefault(al => al.Part)}",
                    hasUndownloaded ? "YES" : "NO");
        }

        Console.WriteLine(table.ToString());
    }

    public void ListDownloadQueue()
    {
        var downloads = database.GetAllDownloads();

        if (downloads.Count == 0)
        {
            Console.WriteLine("No downloads in the queue.");
            return;
        }

        var table = new Table("Episode ID", "URL")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var download in downloads.OrderBy(d => d.Id))
        {
            table.AddRow(download.Id, download.Url);
        }

        Console.WriteLine(table.ToString());   
    }

}

public static class MyEnumerableExtensions
{
    public static int MaxOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int> keySelector, int defaultValue = default(int))
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (source.Any())
        {
            return source.Max(keySelector);
        }

        return defaultValue;
    }

    public static int MinDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int> keySelector, int defaultValue = default(int))
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (keySelector == null)
        {
            throw new ArgumentNullException(nameof(keySelector));
        }

        if (source.Any())
        {
            return source.Min(keySelector);
        }

        return defaultValue;
    }
}
