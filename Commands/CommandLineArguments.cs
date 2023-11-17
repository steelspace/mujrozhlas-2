using CommandLine;

namespace MujRozhlas.CommandLineArguments;

[Verb("list", HelpText = "List information in the database.")]
public class ListOptions
{
}

[Verb("download", HelpText = "Download all episodes that are available.")]
public class DownloadOptions
{
}

[Verb("add", HelpText = "Add serial to the database. Pass an URL from mujrozhlas.cz.")]
public class AddOptions
{
    [Option('u', "url", Required = true, HelpText = "Serial URL from mujrozhlas.cz web site.")]
    public string SerialUrl { get; set; } = String.Empty;
}

[Verb("build", HelpText = "Build audio books from downloaded episodes")]
public class BuildOptions
{
    [Option('i', "id", HelpText = "Serial ID. If omitted, all serials are built")]
    public string SerialId { get; set; } = String.Empty;

    [Option('f', "force", HelpText = "Build serial audio book even if some episodes are missing")]
    public bool Force { get; set; }
}

[Verb("delete", HelpText = "Build audio books from downloaded episodes")]
public class DeleteOptions
{
    [Option('i', "id", Required = true, HelpText = "Serial ID to be removed. User 'list' verb to determine serial ID.")]
    public string SerialId { get; set; } = String.Empty;
}