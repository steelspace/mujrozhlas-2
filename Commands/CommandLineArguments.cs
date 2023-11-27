using CommandLine;

namespace MujRozhlas.CommandLineArguments;

[Verb("list", HelpText = "List information in the database.")]
public class ListOptions
{
    [Option('f', "force", HelpText = "Refresh serials and episodes regardless the last update.")]
    public bool Force { get; set; }

    [Option('i', "incomplete", HelpText = "List only incomplete serials.")]
    public bool IncompleteOnly { get; set; }    
}

[Verb("download", HelpText = "Download all episodes that are available.")]
public class DownloadOptions
{
}

[Verb("add", HelpText = "Add serials to the database. Pass URLs from mujrozhlas.cz.")]
public class AddOptions
{
    [Option('u', "url", Required = true, HelpText = "Space separated serial URLs from mujrozhlas.cz web site.")]
    public string SerialUrls { get; set; } = String.Empty;
}

[Verb("build", HelpText = "Build audio books from downloaded episodes")]
public class BuildOptions
{
    [Option('i', "id", HelpText = "Serial ID. If omitted, all serials are built")]
    public string SerialId { get; set; } = String.Empty;

    [Option('f', "force", HelpText = "Build serial audio book even if some episodes are missing")]
    public bool Force { get; set; }

    [Option('z', "zip", HelpText = "Build audio book as zip file instead of .mp4b.")]
    public bool Zip { get; set; }
}

[Verb("delete", HelpText = "Build audio books from downloaded episodes")]
public class DeleteOptions
{
    [Option('i', "id", Required = true, HelpText = "Space separated serial IDs to be removed. Use 'list' verb to determine serial ID.")]
    public string SerialIds { get; set; } = String.Empty;
}