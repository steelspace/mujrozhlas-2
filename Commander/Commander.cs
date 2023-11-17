using BetterConsoleTables;
using CommunityToolkit.Common;
using Extractor;
using MujRozhlas.Builder;
using MujRozhlas.CommandLineArguments;
using MujRozhlas.Data;
using MujRozhlas.Database;
using MujRozhlas.Lister;
using MujRozhlas.Runner;

namespace MujRozhlas.Commander;

public class Commander
{
    private readonly IDatabase database;
    private readonly IRunner runner;

    private readonly SummaryManager summaryManager;

    public Commander(IDatabase database, IRunner runner)
    {
        this.database = database;
        this.runner = runner;

        summaryManager = new SummaryManager(database);
    }

    public int RunAdd(AddOptions addOptions)
    {
        var parser = new Extractor.TitlePageParser();
        var parsedEpisodes = parser.ExtractTitleInformation(addOptions.SerialUrl).Result;

        var serial = parser.GetSerial(parsedEpisodes).Result;
        Console.WriteLine($"Serial '{serial.ShortTitle}' was recognized.");

        database.SaveSerial(serial);

        GetEpisodes(parser, serial);

        Console.WriteLine($"Serial '{serial.ShortTitle}' was saved into database.");

        return 0;
    }

    int RunRefreshEpisodes()
    {
        Console.WriteLine("Refreshing all episodes.");

        var parser = new Extractor.TitlePageParser();
        var serials = database.GetAllSerials();

        foreach (var serial in serials)
        {
            if ((DateTimeOffset.Now - serial.Updated).TotalHours < 1)
            {
                Console.WriteLine($"Serial '{serial.ShortTitle}' episodes refresh not needed.");
                return 0;
            }

            Console.WriteLine($"Refreshing serial '{serial.ShortTitle}'.");

            GetEpisodes(parser, serial);
        }

        return 0;
    }

    private void GetEpisodes(TitlePageParser parser, Serial serial)
    {
        var episodes = parser.GetAvailableEpisodes(serial.Id).Result;
        database.SaveEpisodes(episodes);
        serial.Updated = DateTimeOffset.Now;
        database.SaveSerial(serial);

        Console.WriteLine($"Serial '{serial.ShortTitle}' has {episodes.Count} parts available from total {serial.TotalParts}.");
    }

    int RunQueue()
    {
        RunRefreshEpisodes();

        var queuer = new Queuer.Queuer(database);
        queuer.QueueAvailableEpisodes();

        return 0;
    }

    public int RunDownload(DownloadOptions queueOptions)
    {
        RunRefreshEpisodes();

        RunQueue();
        
        var downloader = new Downloader.Downloader(database, runner);
        downloader.DownloadAllAudioLinks();
        return 0;
    }    

    public int RunList(ListOptions listOptions)
    {
        RunRefreshEpisodes();

        if (listOptions.Serials)
        {
            summaryManager.ListSerials();
        }

        if (listOptions.DownloadQueue)
        {
            summaryManager.ListDownloadQueue();
        }
        return 0;
    }

    public int RunBuild(BuildOptions opts)
    {
        if (string.IsNullOrEmpty(opts.SerialId))
        {
            var builder = new AudioBookBuilder(database, runner);
            var serials = database.GetAllSerials();

            foreach (var serial in serials)
            {
                builder.BuildBook(serial);
            }
        }

        return 0;
    }
}
