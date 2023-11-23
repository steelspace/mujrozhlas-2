using MujRozhlas.Data;
using MujRozhlas.Database;

namespace MujRozhlas.Queuer;

public class Queuer
{
    private readonly IDatabase database;

    public Queuer(IDatabase database)
    {
        this.database = database;
    }

    public void QueueAvailableEpisodes()
    {
        var serials = database.GetAllSerials();

        if (serials.Count == 0)
        {
            Console.WriteLine("No serial is in the database.");
            return;
        }

        foreach (var serial in serials)
        {
            var episodes = database.GetEpisodes(serial.Id);

            foreach (var episode in episodes.OrderBy(e => e.Part))
            {
                var download = database.GetDownload(episode.Id);

                if (download is not null)
                {
                    Console.WriteLine($"Episode {episode.Part} of {serial.ShortTitle} is already queued for download.");
                    continue;
                }

                var audioLinks = database.GetAudioLinks(episode.Id);
                var audioLink = GetPreferredAudioLink(audioLinks);

                if (audioLink is null)
                {
                    Console.WriteLine($"No audio link found yet for episode {episode.Part} of serial {serial.ShortTitle}");
                    continue;
                }

                download = new Download(episode.Id, audioLink.Url);
                database.InsertDownload(download);

                Console.WriteLine($"Episode {episode.Part} of {serial.ShortTitle} is queued for download.");
            }
        }
    }

    public static AudioLink? GetPreferredAudioLink(List<AudioLink> audioLinks)
    {
        var audioLink = audioLinks.Where(al => al.Variant == "hls").FirstOrDefault();

        if (audioLink is null)
        {
            audioLink = audioLinks.FirstOrDefault();
        }

        return audioLink;
    }
}