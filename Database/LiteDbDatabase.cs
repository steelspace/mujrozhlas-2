using Mujrozhlas.Common;
using Mujrozhlas.Data;
using LiteDB;

namespace Mujrozhlas.Database;
public class LiteDbDatabase : IDatabase, IDisposable
{
    const string fileName = "Mujrozhlas.db";

    LiteDatabase db;

    public LiteDbDatabase()
    {
        db = new LiteDatabase(fileName);
    }

    public Serial LoadSerial(string serialId)
    {
        var episodeCollection = GetSerialDbCollection();
        return episodeCollection.FindById(serialId);
    }

    public void SaveEpisodes(IEnumerable<Episode> episodes)
    {
        var episodeCollection = GetEpisodeDbCollection();
        episodeCollection.Upsert(episodes);

        var audioLinks = episodes.SelectMany(e => e.AudioLinks).ToList();
        var audioLinksCollection = GetAudioLinksDbCollection();
        audioLinksCollection.Upsert(audioLinks);
    }

    public void SaveSerial(Serial serial)
    {
        var episodeCollection = GetSerialDbCollection();
        episodeCollection.Upsert(serial);
    }

    public ILiteCollection<Serial> GetSerialDbCollection() => db.GetCollection<Serial>("serial");
    public ILiteCollection<Episode> GetEpisodeDbCollection() => db.GetCollection<Episode>("episode");
    public ILiteCollection<Download> GetDownloadDbCollection() => db.GetCollection<Download>("download");
    public ILiteCollection<AudioLink> GetAudioLinksDbCollection() => db.GetCollection<AudioLink>("audiolink");

    public void InsertDownload(Download download)
    {
        var downloadCollection = GetDownloadDbCollection();
        downloadCollection.Insert(download);
    }

    public void SetDownloadFinished(string episodeId)
    {
        var downloadCollection = GetDownloadDbCollection();
        var download = downloadCollection.FindById(episodeId);

        if (download == null)
        {
            throw new DownloadedException($"Download {episodeId} doesn't exist");
        }

        download.IsDownloaded = true;
        downloadCollection.Update(download);
    }

    public Download? GetNextDownload()
    {
        throw new NotImplementedException();
    }

    public List<Serial> GetAllSerials()
    {
        using (var db = new LiteDatabase(fileName))
        {
            var episodeCollection = GetSerialDbCollection();
            return episodeCollection.FindAll().ToList();
        }
    }

    public List<Episode> GetEpisodes(string serialId)
    {
        var episodeCollection = GetEpisodeDbCollection();
        var serialEpisodes = episodeCollection.Find(e => e.SerialId == serialId);

        return serialEpisodes.ToList();
    }

    public Download? GetDownload(string episodeId)
    {
        var downloadsCollection = GetDownloadDbCollection();
        var downloads = downloadsCollection.Find(d => d.Id == episodeId);

        if (downloads is null)
        {
            return null;
        }

        return downloads.FirstOrDefault();
    }

    public void Dispose()
    {
        if (db is not null)
        {
            db.Dispose();
        }
    }
}