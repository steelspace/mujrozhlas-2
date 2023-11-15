using Mujrozhlas.Database;
using CommandLine;
using ConsoleTables;
using CommunityToolkit.Common;

internal class Program
{
    public class Options
    {
        [Option('l', "list-requested", Required = false, HelpText = "List of requested serials.")]
        public bool ListRequestdeSerials { get; set; }

        [Option('q', "queue-episodes", Required = false, HelpText = "Queue episodes that are available for download.")]
        public bool QueueEpisodes { get; set; }
    }

    private static void Main(string[] args)
    {
        IDatabase database = new LiteDbDatabase();

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(o =>
            {
                if (o.ListRequestdeSerials)
                {
                    var requestedSerials = database.GetReqestedSerials();
                    var table = new ConsoleTable("Serial", "Parts");
                    
                    foreach (var serial in requestedSerials)
                    {
                        table.AddRow(serial.Title.Truncate(80), serial.TotalParts);
                    }

                    table.Write(Format.Minimal);
                    Console.WriteLine();
                }

                if (o.QueueEpisodes)
                {
                    var requestedSerials = database.GetReqestedSerials();
                    var table = new ConsoleTable("Serial", "Parts");
                    
                    foreach (var serial in requestedSerials)
                    {
                        table.AddRow(serial.Title.Truncate(80), serial.TotalParts);
                    }

                    table.Write(Format.Minimal);
                    Console.WriteLine();
                }                
            });

        // var parser = new Extractor.TitlePageParser();
        // var parsedEpisodes = await parser.ExtractTitleInformation();

        // var serial = await parser.GetSerial(parsedEpisodes);
        // database.SaveSerial(serial);

        // var episodes = await parser.GetAvailableEpisodes(serial.Id);
        // database.SaveEpisodes(episodes);

        // var t = database.LoadSerial(serial.Id);
        // Console.WriteLine(t);
    }
}