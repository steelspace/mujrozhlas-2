using Mujrozhlas.Database;
using CommandLine;
using Mujrozhlas.Commander;

internal class Program
{
    [Verb("list", HelpText = "List of requested serials.")]
    class ListOptions
    {
    }

    [Verb("queue", HelpText = "Queue episodes that are available for download.")]
    class QueueOptions
    {
    }    

    [Verb("add", HelpText = "Add serial to the database. Pass an URL from mujrozhlas.cz.")]
    class AddOptions
    {
        [Option('u', "url", Required = true, HelpText = "Serial URL from mujrozhlas.cz web site.")]
        public string SerialUrl { get; set; } = String.Empty;
    }

    private static void Main(string[] args)
    {
        using IDatabase database = new LiteDbDatabase();
        var commander = new Commander(database);

        Parser.Default.ParseArguments<QueueOptions, ListOptions, AddOptions>(args)
        .MapResult(
            // (Options opts) => Run(opts),
            (AddOptions opts) => commander.RunAdd(opts.SerialUrl),
            (ListOptions opts) => commander.RunList(),
            (QueueOptions opts) => commander.RunQueue(),
            errs => 1);
    }
}