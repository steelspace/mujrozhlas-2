using MujRozhlas.Database;
using CommandLine;
using MujRozhlas.Commander;
using MujRozhlas.CommandLineArguments;
using MujRozhlas.Runner;

internal class Program
{
    private static void Main(string[] args)
    {
        IDatabase database = new LiteDbDatabase();
        IRunner runner = new BashRunner();
        var commander = new Commander(database, runner);

        Parser.Default.ParseArguments<ListOptions, AddOptions, DownloadOptions, BuildOptions, DeleteOptions, PurgeOptions, CleanOptions>(args)
        .MapResult(
            (AddOptions opts) => commander.RunAdd(opts),
            (ListOptions opts) => commander.RunList(opts),
            (DownloadOptions opts) => commander.RunDownload(opts),
            (BuildOptions opts) => commander.RunBuild(opts),
            (DeleteOptions opts) => commander.RunDelete(opts),
            (PurgeOptions opts) => commander.RunPurge(opts),
            (CleanOptions opts) => commander.RunClean(opts),
            errs => 1);
    }
}