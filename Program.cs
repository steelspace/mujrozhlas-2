﻿using MujRozhlas.Database;
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

        Parser.Default.ParseArguments<ListOptions, AddOptions, DownloadOptions, BuildOptions>(args)
        .MapResult(
            (AddOptions opts) => commander.RunAdd(opts),
            (ListOptions opts) => commander.RunList(opts),
            (DownloadOptions opts) => commander.RunDownload(opts),
            (BuildOptions opts) => commander.RunBuild(opts),
            errs => 1);
    }
}