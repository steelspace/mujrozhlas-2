using Mujrozhlas.Data;

namespace Mujrozhlas.Database;
public interface IDatabase
{
    void SaveSerial(Serial serial);
    Serial LoadSerial(string serialId);

    void InsertDownload(Download download);
    void SetDownloadFinished(string episodeId);
    Download GetNextDownload();

    void SaveEpisodes(IEnumerable<Episode> episodes);
}