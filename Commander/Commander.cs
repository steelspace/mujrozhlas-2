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
        var serialUrls = addOptions.SerialUrls.Trim().Split(" ");

        foreach (var serialUrl in serialUrls)
        {
            var parser = new TitlePageParser(database);
            
            // try non serial first by player wrapper
            var parsedEpisode = parser.ExtractTitleInformationFromPlayerWrapper(serialUrl);

            if (parsedEpisode is null)
            {
                // probably serial
                parsedEpisode = parser.ExtractTitleInformation(serialUrl);
            }

            if (parsedEpisode is null)
            {
                Console.WriteLine($"No audio found on '{serialUrl}'.");
                continue;
            }

            var serial = parser.GetSerial(parsedEpisode);

            if (serial is not null)
            {
                Console.WriteLine($"Serial '{serial.ShortTitle}' was recognized.");

                if (database.GetSerial(serial.Id) is not null)
                {
                    Console.WriteLine($"Serial '{serial.ShortTitle}' is already added.");
                    continue;
                }

                database.SaveSerial(serial);
                GetEpisodes(parser, serial);

                Console.WriteLine($"Serial '{serial.ShortTitle}' was saved into database.");
            }
            else
            {
                parsedEpisode = parser.ExtractTitleInformationFromPlayerWrapper(serialUrl);
                var episode = parser.GetNonSerialEpisode(parsedEpisode!.Uuid);

                serial = database.GetSerial(episode.Id);
                if (serial is not null)
                {
                    Console.WriteLine($"Non-serial '{serial.ShortTitle}' is already added.");
                    continue;
                }

                serial = new Serial(episode.Id, episode.Title, episode.ShortTitle, 1, episode.CoveArtUrl, episode.Updated);
                serial.IsNonSerial = true;

                Console.WriteLine($"Non-serial title '{serial.ShortTitle}' was recognized.");

                database.SaveEpisodes(new[] { episode });
                serial.Updated = DateTimeOffset.Now;
                database.SaveSerial(serial);

                Console.WriteLine($"Non-serial title '{serial.ShortTitle}' was saved into database.");
            }
        }

        return 0;
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

    public int RunList(ListOptions listOptions)
    {
        RunRefreshEpisodes(listOptions.Force);
        summaryManager.ListSerials();

        return 0;
    }

    public int RunBuild(BuildOptions opts)
    {
        string serialId = opts.SerialId.Trim();

        var builder = new AudioBookBuilder(database, runner);
        var serials = database.GetAllSerials();

        // filter for serial ID
        if (!string.IsNullOrEmpty(serialId))
        {
            serials = serials.Where(s => s.Id == serialId).ToList();
        }

        if (serials.Count < 1)
        {
            Console.WriteLine($"Serial ID {serialId} was not found.");
        }

        foreach (var serial in serials)
        {
            builder.BuildBook(serial, opts.Force, opts.Zip);
        }

        return 0;
    }

    public int RunDelete(DeleteOptions opts)
    {
        var serialIds = opts.SerialIds.Trim().Split(" ");

        foreach (var serialId in serialIds)
        {
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

        }
        return 0;
    }

    int RunRefreshEpisodes(bool forceRefresh = false)
    {
        Console.WriteLine("Refreshing all episodes.");

        var parser = new TitlePageParser(database);
        var serials = database.GetAllSerials();

        foreach (var serial in serials)
        {
            if (!forceRefresh && (DateTimeOffset.Now - serial.Updated).TotalHours < 1)
            {
                Console.WriteLine($"Serial '{serial.ShortTitle}' episodes refresh not needed.");
                continue;
            }

            Console.WriteLine($"Refreshing serial '{serial.ShortTitle}'.");

            GetEpisodes(parser, serial);
        }

        return 0;
    }

    void GetEpisodes(TitlePageParser parser, Serial serial)
    {
        var episodes = parser.GetAvailableEpisodes(serial.Id);
        database.SaveEpisodes(episodes);
        serial.Updated = DateTimeOffset.Now;
        database.SaveSerial(serial);

        Console.WriteLine($"Serial '{serial.ShortTitle}' has {episodes.Count} parts available from total {serial.TotalParts}.");
    }
}
