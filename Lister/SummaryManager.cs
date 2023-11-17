using BetterConsoleTables;
using CommunityToolkit.Common;
using MujRozhlas.Data;
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

        var table = new Table("Serial", "Id", "Total Parts", "Downloaded", "Available", "Missing", "Book")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var serial in requestedSerials)
        {
            var episodes = database.GetEpisodes(serial.Id);
            var downloaded = episodes.Where(FileManager.IsEpisodeDownloaded);
            var availableAudioLinks = episodes.Where(e => e.AudioLinks.Any(al => al.PlayableTill > DateTimeOffset.Now)).ToArray();

            bool hasUndownloaded =
                downloaded.MaxOrDefault(e => e.Part) < availableAudioLinks.MaxOrDefault(e => e.Part);

            bool isBookReady = FileManager.IsAudioBookReady(serial);
            bool allEpisodesDownloaded = IsSerialCompletelyDownloaded(database, serial);

            table.AddRow(serial.Title.Truncate(30, true), serial.Id, serial.TotalParts,
                    MinMax(downloaded),
                    MinMax(availableAudioLinks),
                    hasUndownloaded ? "YES" : "NO",
                    isBookReady ? "READY" : allEpisodesDownloaded ? "DOWNLOADED" : "INCOMPLETE");
        }

        Console.WriteLine(table.ToString());
    }

    public static bool IsSerialCompletelyDownloaded(IDatabase database, Serial serial)
    {
        var episodes = database.GetEpisodes(serial.Id);

        if (episodes.Count < serial.TotalParts)
        {
            return false;
        }
        
        return episodes.All(e => FileManager.IsEpisodeDownloaded(e));
    }

    static string MinMax(IEnumerable<Episode> episodes)
    {
        int min = episodes.MinDefault(al => al.Part);
        int max = episodes.MaxOrDefault(al => al.Part);
        return $"{(min == 0 ? String.Empty : min)}-{(max == 0 ? String.Empty : max)}";
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
