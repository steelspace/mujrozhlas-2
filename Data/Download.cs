using System.Text.Json.Serialization;

namespace MujRozhlas.Data;

public class Download
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public Download(string id, string url)
    {
        Id = id;
        Url = url;
    }

    [JsonPropertyName("is-downloaded")]
    public bool IsDownloaded { get; set; } = false;

    [JsonPropertyName("url")]
    public string Url { get; set; }
}