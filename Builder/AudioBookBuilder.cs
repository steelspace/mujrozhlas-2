using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MujRozhlas.Data;
using MujRozhlas.Database;
using MujRozhlas.Downloader;
using MujRozhlas.FileManagement;
using MujRozhlas.Lister;
using MujRozhlas.Runner;

namespace MujRozhlas.Builder;

public class AudioBookBuilder
{
    private readonly IDatabase database;
    private readonly IRunner runner;

    public AudioBookBuilder(IDatabase database, IRunner runner)
    {
        this.database = database;
        this.runner = runner;
    }

    public void BuildBook(Serial serial)
    {
        if (FileManager.IsAudioBookReady(serial))
        {
            Console.WriteLine($"Audiobook for '{serial.ShortTitle}' is already built.");
            return;
        }

        if (!SummaryManager.IsSerialCompletelyDownloaded(database, serial))
        {
            Console.WriteLine($"Episodes for '{serial.ShortTitle}' are not downloaded, skipping.");
            return;
        }

        string prefix = new SanitizedFileName(serial.Id).Value;

        var metadataFile = $"{prefix}-metadata.txt";
        var coverArtFile = $"{prefix}-cover.jpg";
        var listFileName = $"{prefix}-list.txt";

        var serialFileNames = CreateList(serial);
        if (string.IsNullOrEmpty(serialFileNames))
        {
            Console.WriteLine($"No downloaded files for serial {serial.ShortTitle}.");
            return;
        }

        string metadata = CreateMetadata(serial);
        string metadataFilePath = FileManager.WriteBuilderTextFile(serial, metadataFile, metadata);
        string coverArtFilePath = FileManager.DownloadImageToOutputFilder(serial, coverArtFile);

        string listFilePath = FileManager.WriteBuilderTextFile(serial, listFileName, serialFileNames);
        string audioBookFileName = FileManager.GetAudioBookFileName(serial);
        string niceAudioBookFileName = FileManager.GetNiceAudioBookFileName(serial);

        if (File.Exists(niceAudioBookFileName))
        {
            Console.WriteLine($"Audiobook '{serial.ShortTitle}' is already generated.");
            return;
        }

        // build
        string buildCommand = String.Format($"ffmpeg -f concat -safe 0 -i {Path.GetFileName(listFilePath)} -i {Path.GetFileName(metadataFilePath)}"
            + $" -vn -y -b:a 128k -acodec aac -ac 2 {Path.GetFileName(audioBookFileName)}");

        string workingFolder = FileManager.GetFullPathToSerialFolder(serial);
        runner.Run(buildCommand, workingFolder);
        
        var titleAuthor = GetTitleAuthor(serial);
        // attach title and cover art
        string attachCommand = $"ffmpeg -y -i {Path.GetFileName(audioBookFileName)} -i {Path.GetFileName(coverArtFilePath)}" 
                        // + (titleAuthor.Item1 is not null ? $" -metadata title=\"{titleAuthor.Item1}\"" : string.Empty)
                        // + (titleAuthor.Item2 is not null ? $" -metadata artist=\"{titleAuthor.Item2}\"" : string.Empty)
                        + $" -map 1 -map 0 -c copy -disposition:0 attached_pic"
                        + $" _{Path.GetFileName(audioBookFileName)}";
                        // + $" && rm {Path.GetFileName(outputFileName)} && mv _{Path.GetFileName(outputFileName)} {Path.GetFileName(outputFileName)}";

        runner.Run(attachCommand, workingFolder);

        string serialFolder = FileManager.GetFullPathToSerialFolder(serial);

        File.Move(Path.Combine(serialFolder, "_" + Path.GetFileName(audioBookFileName)),
             Path.Combine(serialFolder, Path.GetFileName(audioBookFileName)), true);
        FileManager.RenameAudioBook(serial);

        Console.WriteLine($"Audio book {niceAudioBookFileName} created.");
    }

    string? CreateList(Serial serial)
    {
        var episodes = database.GetEpisodes(serial.Id);

        if (episodes.Count == 0)
        {
            return null;
        }

        var fileNames = new List<string>();

        // order is important
        foreach (var episode in episodes.OrderBy(e => e.Part))
        {
            if (FileManager.IsEpisodeDownloaded(episode))
            {
                string filePath = FileManager.GetFileName(episode);
                fileNames.Add(Path.GetFileName(filePath));
            }
        }

        var builder = new StringBuilder();

        foreach (var part in fileNames)
        {
            builder.AppendLine($"file '{part}'");
        }

        return builder.ToString();
    }

    string CreateMetadata(Serial serial)
    {
        // ;FFMETADATA1
        // title=bike\\shed
        // ;this is a comment
        // artist=FFmpeg troll team

        // [CHAPTER]
        // TIMEBASE=1/1000
        // START=0
        // #chapter ends at 0:01:00
        // END=60000
        // title=chapter \#1
        // [STREAM]
        // title=multi\
        // line        

        var builder = new StringBuilder();
        builder.AppendLine(";FFMETADATA1");

        var titleAuthor = GetTitleAuthor(serial);
        builder.AppendLine($"title={NormalizeText(titleAuthor.Item1)}");

        if (titleAuthor.Item2 is not null)
        {
            builder.AppendLine($"artist={titleAuthor.Item2}");
        }

        var episodes = database.GetEpisodes(serial.Id);

        int start = 0;
        foreach (var episode in episodes.OrderBy(p => p.Part))
        {
            var audioLinks = database.GetAudioLinks(episode.Id);
            var audioLink = Queuer.Queuer.GetPreferredAudioLink(audioLinks);

            if (audioLink is null)
            {
                Console.WriteLine($"No audio link found yet for episode {episode.Part} of serial {serial.ShortTitle}");
                continue;
            }

            int end = start + (audioLink.DurationSeconds * 1000);
            var title = episode.ShortTitle;

            builder.AppendLine("[CHAPTER]");
            builder.AppendLine("TIMEBASE=1/1000");
            builder.AppendLine($"START={start}");
            builder.AppendLine($"END={end}");
            builder.AppendLine($"title={NormalizeText(title)}");

            start = end + 1;
        }

        return builder.ToString();
    }

    (string, string?) GetTitleAuthor(Serial serial)
    {
        const string pattern = @"(.*?):(.*)";
        var match = Regex.Match(serial.ShortTitle, pattern);
        
        if (match.Groups.Count == 3)
        {
            string author = match.Groups[1].Value;
            string serialTitle = match.Groups[2].Value;

            return (serialTitle.Trim(), author.Trim());
        }
        else
        {
            return (serial.ShortTitle.Trim(), null);
        }        
    }

    string NormalizeText(string text)
    {
        // ‘=’, ‘;’, ‘#’, ‘\’
        return text.Replace("=", "\\=")
        .Replace(";", "\\;")
        .Replace("#", "\\#")
        .Replace("\\", "\\\\");
    }
}