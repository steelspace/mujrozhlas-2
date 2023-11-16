using System.Text.Json.Serialization;

namespace MujRozhlas.Data;
public class Serial
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public Serial(string id, string title, string shortTitle, int totalParts, string coverArtUrl)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        TotalParts = totalParts;
        CoverArtUrl = coverArtUrl;
    }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("short-title")]
    public string ShortTitle { get; set; }

    [JsonPropertyName("total-parts")]
    public int TotalParts { get; set; }

    [JsonPropertyName("cover-art-url")]
    public string CoverArtUrl { get; set; }
}