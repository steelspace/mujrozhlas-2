using Mujrozhlas.Data;
using Mujrozhlas.Database;

namespace Mujrozhlas.Queuer;

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

        foreach (var serial in serials)
        {
            var episodes = database.GetEpisodes(serial.Id);

            foreach (var episode in episodes)
            {
                var download = database.GetDownload(episode.Id);

                if (download is null)
                {
                    var audioLinks = database.GetAudioLinks(episode.Id);
                    var audioLink = audioLinks.Where(al => al.Variant == "hls").FirstOrDefault();

                    if (audioLink is null)
                    {
                        audioLink = episode.AudioLinks.FirstOrDefault();
                    }

                    if (audioLink is null)
                    {
                        Console.WriteLine($"No audio link found for episode {episode.Id} of serial {episode.SerialId}");
                        continue;
                    }

                    download = new Download(episode.Id, audioLink.Url);
                    database.InsertDownload(download);

                    Console.WriteLine($"URL {download.Url} for episode {episode.Part} of {serial.ShortTitle}");
                }
            }
        }
    }
}