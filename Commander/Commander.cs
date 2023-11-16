using BetterConsoleTables;
using CommunityToolkit.Common;
using MujRozhlas.Builder;
using MujRozhlas.CommandLineArguments;
using MujRozhlas.Database;
using MujRozhlas.Runner;

namespace MujRozhlas.Commander;

public class Commander
{
    private readonly IDatabase database;
    private readonly IRunner runner;

    public Commander(IDatabase database, IRunner runner)
    {
        this.database = database;
        this.runner = runner;
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

    public int RunDownload(DownloadOptions queueOptions)
    {
        var downloader = new Downloader.Downloader(database, runner);
        downloader.DownloadAllAudioLinks();
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

        if (requestedSerials.Count == 0)
        {
            Console.WriteLine("No serials are requested.");
            return;
        }

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
