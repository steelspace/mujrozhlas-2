using System.Text.Json.Serialization;

namespace Data;
public class Episode
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public Episode(string id, string title, string shortTitle, int part)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        Part = part;
    }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("short-title")]
    public string ShortTitle { get; set; }

    [JsonPropertyName("part")]
    public int Part { get; set; }

    [JsonPropertyName("audio-links")]
    public List<AudioLink> AudioLinks  { get; } = new List<AudioLink>();
}