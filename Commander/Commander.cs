using BetterConsoleTables;
using CommunityToolkit.Common;
using Mujrozhlas.Database;

namespace Mujrozhlas.Commander;

public class Commander
{
    private readonly IDatabase database;

    public Commander(IDatabase database)
    {
        this.database = database;
    }

    public int RunAdd(string url)
    {
        var parser = new Extractor.TitlePageParser();
        var parsedEpisodes = parser.ExtractTitleInformation(url).Result;

        var serial = parser.GetSerial(parsedEpisodes).Result;
        Console.WriteLine($"Serial '{serial.ShortTitle}' was recognized.");

        database.SaveSerial(serial);

        var episodes = parser.GetAvailableEpisodes(serial.Id).Result;
        database.SaveEpisodes(episodes);
        Console.WriteLine($"Serial '{serial.ShortTitle}' has {episodes.Count} parts available from total {serial.TotalParts}.");

        Console.WriteLine($"Serial '{serial.ShortTitle}' was saved into database.");

        return 0;
    }

    public int RunQueue()
    {
        var queuer = new Queuer.Queuer(database);
        queuer.QueueAvailableEpisodes();

        return 0;
    }

    public int RunList()
    {
        var requestedSerials = database.GetAllSerials();
        var table = new Table("Serial", "Id", "Parts");
        table.Config = TableConfiguration.Markdown();

        foreach (var serial in requestedSerials)
        {
            table.AddRow(serial.Title.Truncate(80, true), serial.Id, serial.TotalParts);
        }

        Console.Write(table.ToString());      
        return 0;
    }
}
