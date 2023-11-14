using Data;
using LiteDB;

namespace Database;
public class LiteDbDatabase : IDatabase
{
    const string fileName = "Mujrozhlas.db";

    public Serial LoadSerial(string serialId)
    {
        using(var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetSerialDbCollection(db);
            return episodeCollection.FindById(serialId);
        }        
    }

    public void SaveEpisodes(IEnumerable<Episode> episodes)
    {
        using(var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetEpisodeDbCollection(db);
            episodeCollection.Upsert(episodes);
        }
    }

    public void SaveSerial(Serial serial)
    {
        using(var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetSerialDbCollection(db);
            episodeCollection.Upsert(serial);
        }
    }

    public ILiteCollection<Serial> GetSerialDbCollection(LiteDatabase db) => db.GetCollection<Serial>("serial");
    public ILiteCollection<Episode> GetEpisodeDbCollection(LiteDatabase db) => db.GetCollection<Episode>("episode");

}