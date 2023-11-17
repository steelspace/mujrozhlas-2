using MujRozhlas.Data;

namespace MujRozhlas.Database;
public interface IDatabase
{
    void SaveSerial(Serial serial);
    Serial GetSerial(string serialId);

    void InsertDownload(Download download);
    void SetDownloadFinished(string episodeId);
    Download? GetDownload(string episodeId);

    void SaveEpisodes(IEnumerable<Episode> episodes);

    List<Serial> GetAllSerials();
    List<AudioLink> GetAllAudioLinks();
    List<Download> GetAllDownloads();

    List<Episode> GetEpisodes(string serialId);
    List<AudioLink> GetAudioLinks(string episodeId);
    Episode GetEpisode(string episodeId);
}