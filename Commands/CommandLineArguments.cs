using CommandLine;

namespace Mujrozhlas.CommandLineArguments;

[Verb("list", HelpText = "List information in the database.")]
public class ListOptions
{
    [Option('s', "serials", HelpText = "List requested serials.")]
    public bool Serials { get; set; }

    [Option('d', "download-queue", HelpText = "List episodes in the download queue.")]
    public bool DownloadQueue { get; set; }
}

[Verb("queue", HelpText = "Put all available episodes to the download queue.")]
public class QueueOptions
{
}

[Verb("download", HelpText = "Download all queued episodes that are available.")]
public class DownloadOptions
{
}

[Verb("add", HelpText = "Add serial to the database. Pass an URL from mujrozhlas.cz.")]
public class AddOptions
{
    [Option('u', "url", Required = true, HelpText = "Serial URL from mujrozhlas.cz web site.")]
    public string SerialUrl { get; set; } = String.Empty;
}
