using MujRozhlas.Data;
using LiteDB;

namespace MujRozhlas.Database;
public class LiteDbDatabase : IDatabase
{
    const string fileName = "Mujrozhlas.db";

    public Serial GetSerial(string serialId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetSerialDbCollection(db);
            return episodeCollection.FindById(serialId);
        }
    }

    public void SaveEpisodes(IEnumerable<Episode> episodes)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetEpisodeDbCollection(db);
            episodeCollection.Upsert(episodes);
        }
    }

    public void SaveSerial(Serial serial)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var serialsCollection = GetSerialDbCollection(db);
            serialsCollection.Upsert(serial);
        }
    }

    public ILiteCollection<Serial> GetSerialDbCollection(LiteDatabase db) => db.GetCollection<Serial>("serial");
    public ILiteCollection<Episode> GetEpisodeDbCollection(LiteDatabase db) => db.GetCollection<Episode>("episode");
    public ILiteCollection<Download> GetDownloadDbCollection(LiteDatabase db) => db.GetCollection<Download>("download");

    public void InsertDownload(Download download)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var downloadCollection = GetDownloadDbCollection(db);
            downloadCollection.Insert(download);
        }
    }

    public void SetDownloadFinished(string episodeId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var downloadCollection = GetDownloadDbCollection(db);
            var download = downloadCollection.FindById(episodeId);

            download.IsDownloaded = true;
            downloadCollection.Update(download);

            var episodesCollection = GetEpisodeDbCollection(db);
            var episode = episodesCollection.FindById(episodeId);

            episode.IsDownloaded = true;
            episodesCollection.Update(episode);
        }
    }

    public List<Serial> GetAllSerials()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var serialCollection = GetSerialDbCollection(db);
            return serialCollection.FindAll().ToList();
        }
    }

    public List<Episode> GetEpisodes(string serialId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetEpisodeDbCollection(db);
            var serialEpisodes = episodeCollection.Find(e => e.SerialId == serialId);

            return serialEpisodes.ToList();
        }
    }

    public Download? GetDownload(string episodeId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var downloadsCollection = GetDownloadDbCollection(db);
            var downloads = downloadsCollection.Find(d => d.Id == episodeId);

            if (downloads is null)
            {
                return null;
            }

            return downloads.FirstOrDefault();
        }
    }

    public List<AudioLink> GetAudioLinks(string episodeId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episode = GetEpisode(episodeId);
            return episode.AudioLinks;
        }
    }

    public List<AudioLink> GetAllAudioLinks()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodes = GetAllEpisodes();

            return episodes.SelectMany(e => e.AudioLinks).ToList();
        }
    }

    public List<Episode> GetAllEpisodes()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodesCollection = GetEpisodeDbCollection(db);
            return episodesCollection.FindAll().ToList();
        }
    }

    public List<Download> GetAllDownloads()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var downloadsCollection = GetDownloadDbCollection(db);
            return downloadsCollection.FindAll().ToList();
        }
    }

    public Episode GetEpisode(string episodeId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodesCollection = GetEpisodeDbCollection(db);
            return episodesCollection.FindById(episodeId);
        }
    }

    public void DeleteSerial(string serialId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var serialCollection = GetSerialDbCollection(db);
            serialCollection.Delete(serialId);
        }
    }


    public void DeleteSerialEpisodes(string serialId)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodesCollection = GetEpisodeDbCollection(db);
            episodesCollection.DeleteMany(e => e.SerialId == serialId);
        }
    }
}