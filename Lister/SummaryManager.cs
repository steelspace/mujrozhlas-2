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

    public void ListSerials(bool incompleteOnly)
    {
        var requestedSerials = database.GetAllSerials();

        if (requestedSerials.Count == 0)
        {
            Console.WriteLine("No serials are requested.");
            return;
        }

        var table = new Table("Serial", "Id", "Total", "Down.", "Book", "Available")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var serial in requestedSerials.OrderBy(s => s.ShortTitle))
        {
            var episodes = database.GetEpisodes(serial.Id);
            var downloaded = episodes.Where(FileManager.IsEpisodeDownloaded);
            var availableAudioLinks = episodes.Where(e => e.AudioLinks.Any(al => al.PlayableTill > DateTimeOffset.Now)).ToArray();

            bool hasUndownloaded =
                downloaded.MaxOrDefault(e => e.Part) < availableAudioLinks.MaxOrDefault(e => e.Part);

            bool isGone = availableAudioLinks.Length == 0 
                || (downloaded.Any() && downloaded.MaxOrDefault(e => e.Part) < availableAudioLinks.MinDefault(e => e.Part));
            bool isBookReady = FileManager.IsAudioBookReady(serial);

            if (isBookReady && incompleteOnly)
            {
                continue;
            }

            DateTimeOffset? playableTill = null;
            int? availableInDays = null;

            if (hasUndownloaded)
            {
                playableTill = episodes.SelectMany(e => e.AudioLinks).Max(a => a.PlayableTill);
                var playableFor = playableTill - DateTimeOffset.Now;

                if (playableFor is not null)
                {
                    availableInDays = (int)playableFor.Value.TotalDays;
                }
            }

            bool allEpisodesDownloaded = IsSerialCompletelyDownloaded(database, serial);

            table.AddRow(serial.Title.Truncate(40, true), serial.Id, serial.TotalParts,
                    MinMax(downloaded, serial.TotalParts),
                    isBookReady ? "BOOK CREATED" : allEpisodesDownloaded ? "ALL EPISODES" : "INCOMPLETE",
                    availableInDays is not null ? availableInDays > 10000 ? "UNRESTRICTED" : availableInDays + " days" : String.Empty);
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

    static string MinMax(IEnumerable<Episode> episodes, int totalParts)
    {
        int min = episodes.MinDefault(al => al.Part);
        int max = episodes.MaxOrDefault(al => al.Part);

        var missingEpisodes = GetMissingNumbers(totalParts, episodes.Select(e => e.Part));

        string baseText = $"{(min == 0 ? String.Empty : min)}-{(max == 0 ? String.Empty : max)}";

        if (missingEpisodes.Count > 0 && missingEpisodes.Max() < episodes.MaxOrDefault(e => e.Part))
        {
            baseText += $" (Miss {string.Join(",", missingEpisodes)})";
        }

        return baseText;
    }

    static List<int> GetMissingNumbers(int maxNumber, IEnumerable<int> inputList)
    {
        var fullSet = new HashSet<int>(Enumerable.Range(1, maxNumber));

        foreach (var num in inputList)
        {
            fullSet.Remove(num);
        }

        return fullSet.ToList();
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
