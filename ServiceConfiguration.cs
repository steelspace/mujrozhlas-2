using Microsoft.Extensions.DependencyInjection;
using Extractor;
using MujRozhlas.Database;
using MujRozhlas.Commands;
using MujRozhlas.Runner;

namespace MujRozhlas;

public static class ServiceConfiguration
{
    public static IServiceCollection AddMujRozhlasServices(this IServiceCollection services, string? databasePath = null)
    {
        // Register database with proper initialization
        services.AddSingleton<IDatabase>(provider =>
        {
            // 'LiteDbDatabase' does not have a constructor that takes a path.
            // Using the parameterless constructor as indicated by previous code.
            return new LiteDbDatabase();
        });

        // Register runner
        services.AddSingleton<IRunner, BashRunner>();

        // Register HttpClient for TitlePageParser with proper headers
        services.AddHttpClient<TitlePageParser>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            return new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };
        });

        // Register Commander
        services.AddTransient<Commander>();

        return services;
    }
}
