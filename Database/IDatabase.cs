using Mujrozhlas.Data;

namespace Mujrozhlas.Database;
public interface IDatabase : IDisposable
{
    void SaveSerial(Serial serial);
    Serial LoadSerial(string serialId);

    void InsertDownload(Download download);
    void SetDownloadFinished(string episodeId);
    Download? GetDownload(string episodeId);
    Download? GetNextDownload();

    void SaveEpisodes(IEnumerable<Episode> episodes);

    List<Serial> GetAllSerials();

    List<Episode> GetEpisodes(string serialId);
}