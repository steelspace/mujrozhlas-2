using System.Text.Json.Serialization;

namespace Mujrozhlas.Data;

public class Download
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public Download(string id)
    {
        Id = id;
    }

    [JsonPropertyName("is-downloaded")]
    public bool IsDownloaded { get; set; } = false;
}