using Mujrozhlas.Common;
using Mujrozhlas.Data;
using LiteDB;

namespace Mujrozhlas.Database;
public class LiteDbDatabase : IDatabase
{
    const string fileName = "Mujrozhlas.db";

    public Serial LoadSerial(string serialId)
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

            var audioLinks = episodes.SelectMany(e => e.AudioLinks).ToList();
            var audioLinksCollection = GetAudioLinksDbCollection(db);
            audioLinksCollection.Upsert(audioLinks);
        }
    }

    public void SaveSerial(Serial serial)
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetSerialDbCollection(db);
            episodeCollection.Upsert(serial);
        }
    }

    public ILiteCollection<Serial> GetSerialDbCollection(LiteDatabase db) => db.GetCollection<Serial>("serial");
    public ILiteCollection<Episode> GetEpisodeDbCollection(LiteDatabase db) => db.GetCollection<Episode>("episode");
    public ILiteCollection<Download> GetDownloadDbCollection(LiteDatabase db) => db.GetCollection<Download>("download");
    public ILiteCollection<AudioLink> GetAudioLinksDbCollection(LiteDatabase db) => db.GetCollection<AudioLink>("audiolink");

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

            if (download == null)
            {
                throw new DownloadedException($"Download {episodeId} doesn't exist");
            }

            download.IsDownloaded = true;
            downloadCollection.Update(download);
        }
    }

    public Download? GetNextDownload()
    {
        throw new NotImplementedException();
    }

    public List<Serial> GetAllSerials()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetSerialDbCollection(db);
            return episodeCollection.FindAll().ToList();
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
            var audioLinksCollection = GetAudioLinksDbCollection(db);
            return audioLinksCollection.Find(d => d.EpisodeId == episodeId).ToList();
        }
    }

    public List<AudioLink> GetAllAudioLinks()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var audioLinksCollection = GetAudioLinksDbCollection(db);
            return audioLinksCollection.FindAll().ToList();
        }
    }
}