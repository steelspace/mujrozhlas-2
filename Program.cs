using CommandLine;
using MujRozhlas.Commands;
using MujRozhlas.CommandLineArguments;
using Microsoft.Extensions.DependencyInjection;
using MujRozhlas;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        // Setup DI container
        var services = new ServiceCollection();
        services.AddMujRozhlasServices();
        await using var serviceProvider = services.BuildServiceProvider();

        try
        {
            var commander = serviceProvider.GetRequiredService<Commander>();

            return await Parser.Default.ParseArguments<ListOptions, AddOptions, DownloadOptions, BuildOptions, DeleteOptions, PurgeOptions, CleanOptions>(args)
                .MapResult(
                    (AddOptions opts) => commander.RunAddAsync(opts),
                    (ListOptions opts) => commander.RunListAsync(opts),
                    (DownloadOptions opts) => commander.RunDownloadAsync(opts),
                    (BuildOptions opts) => Task.FromResult(commander.RunBuild(opts)),
                    (DeleteOptions opts) => Task.FromResult(commander.RunDelete(opts)),
                    (PurgeOptions opts) => Task.FromResult(commander.RunPurge(opts)),
                    (CleanOptions opts) => commander.RunCleanAsync(opts),
                    errs => Task.FromResult(1));
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            Console.ResetColor();
            return 1;
        }
    }
}