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

        var table = new Table("Serial", "Id", "Total Parts", "Downloaded", "Available")
        {
            Config = TableConfiguration.Markdown()
        };

        foreach (var serial in requestedSerials)
        {
            var episodes = database.GetEpisodes(serial.Id);
            int downloaded = episodes.Where(FileManager.IsEpisodeDownloaded).Count();
            int available = episodes.Where(e => e.AudioLinks.Count > 0).Count();

            table.AddRow(serial.Title.Truncate(30, true), serial.Id, serial.TotalParts, downloaded, available);
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