using Extractor;
using MujRozhlas.Builder;
using MujRozhlas.CommandLineArguments;
using MujRozhlas.Data;
using MujRozhlas.Database;
using MujRozhlas.FileManagement;
using MujRozhlas.Lister;
using MujRozhlas.Runner;

namespace MujRozhlas.Commands;

public class Commander
{
    private readonly IDatabase database;
    private readonly IRunner runner;
    private readonly TitlePageParser parser;

    private readonly SummaryManager summaryManager;

    public Commander(IDatabase database, IRunner runner, TitlePageParser parser)
    {
        this.database = database;
        this.runner = runner;
        this.parser = parser;

        summaryManager = new SummaryManager(database);
    }

    public async Task<int> RunAddAsync(AddOptions addOptions)
    {
        var serialUrls = addOptions.SerialUrls.Trim().Split(" ");

        foreach (var serialUrl in serialUrls)
        {
            // try non serial first by player wrapper
            var parsedEpisode = await parser.ExtractTitleInformationFromPlayerWrapperAsync(serialUrl);

            if (parsedEpisode is null)
            {
                // probably serial
                parsedEpisode = await parser.ExtractTitleInformationAsync(serialUrl);
            }

            if (parsedEpisode is null)
            {
                Console.WriteLine($"No audio found on '{serialUrl}'.");
                continue;
            }

            var serial = await parser.GetSerialAsync(parsedEpisode);

            if (serial is not null)
            {
                Console.WriteLine($"Serial '{serial.ShortTitle}' was recognized.");

                if (database.GetSerial(serial.Id) is not null)
                {
                    Console.WriteLine($"Serial '{serial.ShortTitle}' is already added.");
                    continue;
                }

                database.SaveSerial(serial);
                await GetEpisodesAsync(serial);

                Console.WriteLine($"Serial '{serial.ShortTitle}' was saved into database.");
            }
            else
            {
                parsedEpisode = await parser.ExtractTitleInformationFromPlayerWrapperAsync(serialUrl);

                if (parsedEpisode is null)
                {
                    Console.WriteLine($"URL '{serialUrl}' contains no supported show.");
                    return -1;
                }

                var episode = await parser.GetNonSerialEpisodeAsync(parsedEpisode!.Uuid);

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

    async Task<int> RunQueueAsync()
    {
        await RunRefreshEpisodesAsync();

        var queuer = new Queuer.Queuer(database);
        queuer.QueueAvailableEpisodes();

        return 0;
    }

    public async Task<int> RunDownloadAsync(DownloadOptions _)
    {
        await RunRefreshEpisodesAsync();
        await RunQueueAsync();

        var downloader = new Downloader.Downloader(database, runner);
        downloader.DownloadAllAudioLinks();
        return 0;
    }

    public async Task<int> RunListAsync(ListOptions listOptions)
    {
        await RunRefreshEpisodesAsync(listOptions.Force);
        summaryManager.ListSerials(listOptions.IncompleteOnly);

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
                DeleteSerial(serial);
            }

        }
        return 0;
    }

    public int RunPurge(PurgeOptions opts)
    {
        var serials = database.GetAllSerials();

        foreach (var serial in serials)
        {
            if (FileManager.IsAudioBookReady(serial))
            {
                DeleteSerial(serial);
            }
        }
        return 0;
    }

    public async Task<int> RunCleanAsync(CleanOptions opts)
    {
        Console.WriteLine("Cleaning all failed downloads.");

        var serials = database.GetAllSerials();

        foreach (var serial in serials)
        {
            var episodes = await parser.GetAvailableEpisodesAsync(serial.Id);

            foreach (var episode in episodes)
            {
                FileManager.DeleteFailedDownload(episode);
            }
        }

        return 0;
    }

    void DeleteSerial(Serial serial)
    {
        database.DeleteSerialEpisodes(serial.Id);
        database.DeleteSerial(serial.Id);
        FileManager.DeleteSerialFiles(serial.Id);

        Console.WriteLine($"Serial '{serial.ShortTitle}' was deleted.");
    }

    async Task<int> RunRefreshEpisodesAsync(bool forceRefresh = false)
    {
        Console.WriteLine("Refreshing all episodes.");

        var serials = database.GetAllSerials();

        foreach (var serial in serials)
        {
            if (!forceRefresh && (DateTimeOffset.Now - serial.Updated).TotalHours < 1)
            {
                Console.WriteLine($"Serial '{serial.ShortTitle}' episodes refresh not needed.");
                continue;
            }

            Console.WriteLine($"Refreshing serial '{serial.ShortTitle}'.");

            await GetEpisodesAsync(serial);
        }

        return 0;
    }

    async Task GetEpisodesAsync(Serial serial)
    {
        var episodes = await parser.GetAvailableEpisodesAsync(serial.Id);
        database.SaveEpisodes(episodes);
        serial.Updated = DateTimeOffset.Now;
        database.SaveSerial(serial);

        Console.WriteLine($"Serial '{serial.ShortTitle}' has {episodes.Count} parts available from total {serial.TotalParts}.");
    }
}
