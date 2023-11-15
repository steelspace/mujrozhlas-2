using LiteDB;

namespace Mujrozhlas.Data;

public class Download
{
    [BsonField("id")]
    public string Id { get; set; }

    public Download(string id, string url)
    {
        Id = id;
        Url = url;
    }

    [BsonField("is-downloaded")]
    public bool IsDownloaded { get; set; } = false;

    [BsonField("url")]
    public string Url { get; set; }
}