using System.Text.Json.Serialization;

namespace MujRozhlas.Data;
public class Serial
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public Serial(string id, string title, string shortTitle, int totalParts, string coverArtUrl, DateTimeOffset updated)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        TotalParts = totalParts;
        CoverArtUrl = coverArtUrl;
        Updated = updated;
    }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("short-title")]
    public string ShortTitle { get; set; }

    [JsonPropertyName("total-parts")]
    public int TotalParts { get; set; }

    [JsonPropertyName("non-serial")]
    public bool IsNonSerial { get; set; } = false;

    [JsonPropertyName("cover-art-url")]
    public string CoverArtUrl { get; set; }

    [JsonPropertyName("updated")]
    public DateTimeOffset Updated { get; set; } = DateTimeOffset.MinValue;
}