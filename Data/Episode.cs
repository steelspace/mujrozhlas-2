using System.Text.Json.Serialization;
namespace MujRozhlas.Data;

public class Episode : IPart
{
    public Episode() {}
    
    public Episode(string id, string title, string shortTitle, int part, string serialId, DateTimeOffset since, DateTimeOffset till, DateTimeOffset updated, string coverArtUrl = "")
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        Part = part;
        SerialId = serialId;
        Since = since;
        Till = till;
        Updated = updated;
        CoveArtUrl = coverArtUrl;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; } = String.Empty;

    [JsonPropertyName("serial-id")]
    public string SerialId { get; set; } = String.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = String.Empty;
    
    [JsonPropertyName("short-title")]
    public string ShortTitle { get; set; } = String.Empty;

    [JsonPropertyName("cover-art-url")]
    public string CoveArtUrl { get; set; } = String.Empty;

    [JsonPropertyName("part")]
    public int Part { get; set; }

    [JsonPropertyName("audio-links")]
    public List<AudioLink> AudioLinks  { get; set; } = new List<AudioLink>();

    [JsonPropertyName("since")]
    public DateTimeOffset Since { get; set; }

    [JsonPropertyName("till")]
    public DateTimeOffset Till { get; set; }

    [JsonPropertyName("updated")]
    public DateTimeOffset Updated { get; set; }

    [JsonPropertyName("is-downloaded")]
    public bool IsDownloaded { get; set; }
}