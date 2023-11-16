using System.Text.Json.Serialization;
namespace Mujrozhlas.Data;
public class Episode
{
    public Episode(string id, string title, string shortTitle, int part, string serialId)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        Part = part;
        SerialId = serialId;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("serial-id")]
    public string SerialId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("short-title")]
    public string ShortTitle { get; set; }

    [JsonPropertyName("part")]
    public int Part { get; set; }

    [JsonPropertyName("audio-links")]
    public List<AudioLink> AudioLinks  { get; set; } = new List<AudioLink>();

    [JsonPropertyName("is-downloaded")]
    public bool IsDownloaded { get; set; }
}