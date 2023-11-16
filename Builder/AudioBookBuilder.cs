using System.Text;
using MujRozhlas.Data;
using MujRozhlas.Database;
using MujRozhlas.Downloader;

namespace MujRozhlas.Builder;

public class AudioBookBuilder
{
    private readonly IDatabase database;

    public AudioBookBuilder(IDatabase database)
    {
        this.database = database;
    }

    public void BuildBook(Serial serial)
    {
        string prefix = new SanitizedFileName(serial.ShortTitle).Value;

        var metadataFile = $"{prefix}-metadata.txt";
        var coverArtFile = $"{prefix}-cover.jpg";
        var outputFileName = $"{prefix}-book.mp4";
        var listFileName = $"{prefix}-list.txt";

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
        builder.AppendLine($"title={NormalizeText(serial.ShortTitle)}");

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

    string NormalizeText(string text)
    {
        // ‘=’, ‘;’, ‘#’, ‘\’
        return text.Replace("=", "\\=")
        .Replace(";", "\\;")
        .Replace("#", "\\#")
        .Replace("\\", "\\\\");
    }
}