using Mujrozhlas.Database;
using CommandLine;
using Mujrozhlas.Commander;
using Mujrozhlas.CommandLineArguments;
using Mujrozhlas.Runner;

internal class Program
{
    private static void Main(string[] args)
    {
        IDatabase database = new LiteDbDatabase();
        IRunner runner = new BashRunner();
        var commander = new Commander(database, runner);

        Parser.Default.ParseArguments<QueueOptions, ListOptions, AddOptions, DownloadOptions>(args)
        .MapResult(
            (AddOptions opts) => commander.RunAdd(opts),
            (ListOptions opts) => commander.RunList(opts),
            (QueueOptions opts) => commander.RunQueue(opts),
            (DownloadOptions opts) => commander.RunDownload(opts),
            errs => 1);
    }
}