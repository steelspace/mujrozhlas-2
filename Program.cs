using Mujrozhlas.Database;
using CommandLine;
using Mujrozhlas.Commander;
using Mujrozhlas.CommandLineArguments;

internal class Program
{
    private static void Main(string[] args)
    {
        IDatabase database = new LiteDbDatabase();
        var commander = new Commander(database);

        Parser.Default.ParseArguments<QueueOptions, ListOptions, AddOptions>(args)
        .MapResult(
            (AddOptions opts) => commander.RunAdd(opts),
            (ListOptions opts) => commander.RunList(opts),
            (QueueOptions opts) => commander.RunQueue(opts),
            errs => 1);
    }
}