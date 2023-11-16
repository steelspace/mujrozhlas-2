using BetterConsoleTables;
using CommunityToolkit.Common;
using Mujrozhlas.CommandLineArguments;
using Mujrozhlas.Database;

namespace Mujrozhlas.Commander;

public class Commander
{
    private readonly IDatabase database;

    public Commander(IDatabase database)
    {
        this.database = database;
    }

    public int RunAdd(AddOptions addOptions)
    {
        var parser = new Extractor.TitlePageParser();
        var parsedEpisodes = parser.ExtractTitleInformation(addOptions.SerialUrl).Result;

        var serial = parser.GetSerial(parsedEpisodes).Result;
        Console.WriteLine($"Serial '{serial.ShortTitle}' was recognized.");

        database.SaveSerial(serial);

        var episodes = parser.GetAvailableEpisodes(serial.Id).Result;
        database.SaveEpisodes(episodes);
        Console.WriteLine($"Serial '{serial.ShortTitle}' has {episodes.Count} parts available from total {serial.TotalParts}.");

        Console.WriteLine($"Serial '{serial.ShortTitle}' was saved into database.");

        return 0;
    }

    public int RunQueue(QueueOptions queueOptions)
    {
        var queuer = new Queuer.Queuer(database);
        queuer.QueueAvailableEpisodes();

        return 0;
    }

    public int RunList(ListOptions listOptions)
    {
        if (listOptions.Serials)
        {
            ListSerials();
        }

        if (listOptions.DownloadQueue)
        {
            ListDownloadQueue();
        }
        return 0;
    }

    void ListSerials()
    {
        var requestedSerials = database.GetAllSerials();
        var table = new Table("Serial", "Id", "Parts")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var serial in requestedSerials)
        {
            table.AddRow(serial.Title.Truncate(80, true), serial.Id, serial.TotalParts);
        }

        Console.WriteLine(table.ToString());      
    }

    void ListDownloadQueue()
    {
        var audioLinks = database.GetAllAudioLinks();

        var table = new Table("Episode ID", "URL")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var audioLink in audioLinks.OrderBy(a => a.EpisodeId))
        {
            table.AddRow(audioLink.EpisodeId, audioLink.Url);
        }

        Console.WriteLine(table.ToString());   
    }
}
