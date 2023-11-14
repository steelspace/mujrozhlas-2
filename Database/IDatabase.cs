using Data;

namespace Database;
public interface IDatabase
{
    void SaveSerial(Serial serial);
    Serial LoadSerial(string serialId);

    void SaveEpisodes(IEnumerable<Episode> episodes);
}