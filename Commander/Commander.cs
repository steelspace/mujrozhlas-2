using Extractor;
using MujRozhlas.Builder;
using MujRozhlas.CommandLineArguments;
using MujRozhlas.Data;
using MujRozhlas.Database;
using MujRozhlas.FileManagement;
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
        string serialUrl = addOptions.SerialUrl.Trim();

        var parser = new Extractor.TitlePageParser();
        var parsedEpisode = parser.ExtractTitleInformation(serialUrl);

        var serial = parser.GetSerial(parsedEpisode);

        if (serial is not null)
        {
            Console.WriteLine($"Serial '{serial.ShortTitle}' was recognized.");

            if (database.GetSerial(serial.Id) != null)
            {
                Console.WriteLine($"Serial '{serial.ShortTitle}' is already added.");
                return 0;
            }

            database.SaveSerial(serial);
            GetEpisodes(parser, serial);

            Console.WriteLine($"Serial '{serial.ShortTitle}' was saved into database.");
        }
        else
        {
            parsedEpisode = parser.ExtractNonSerialTitleInformation(serialUrl);
            var episode = parser.GetNonSerialEpisode(parsedEpisode.Uuid);
            serial = new Serial(episode.Id, episode.Title, episode.ShortTitle, 1, String.Empty, episode.Updated);

            database.SaveEpisodes(new [] { episode });
            serial.Updated = DateTimeOffset.Now;
            database.SaveSerial(serial);
        }

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
        var episodes = parser.GetAvailableEpisodes(serial.Id);
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

    public int RunDownload(DownloadOptions _)
    {
        RunRefreshEpisodes();
        RunQueue();
        
        var downloader = new Downloader.Downloader(database, runner);
        downloader.DownloadAllAudioLinks();
        return 0;
    }    

    public int RunList(ListOptions _)
    {
        RunRefreshEpisodes();
        summaryManager.ListSerials();

        return 0;
    }

    public int RunBuild(BuildOptions opts)
    {
        string serialId = opts.SerialId.Trim();

        if (string.IsNullOrEmpty(serialId))
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

    public int RunDelete(DeleteOptions opts)
    {
        string serialId = opts.SerialId.Trim();
        var serial = database.GetSerial(serialId);

        if (serial == null)
        {
            Console.WriteLine($"Serial '{serialId}' was not found.");
            return 0;
        }
        else
        {
            database.DeleteSerialEpisodes(serialId);
            database.DeleteSerial(serialId);
            FileManager.DeleteSerialFiles(serialId);

            Console.WriteLine($"Serial '{serial.ShortTitle}' was deleted.");
        }

        return 0;
    }
}
