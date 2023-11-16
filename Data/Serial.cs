using System.Text.Json.Serialization;

namespace MujRozhlas.Data;
public class Serial
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    public Serial(string id, string title, string shortTitle, int totalParts)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        TotalParts = totalParts;
    }

    [JsonPropertyName("title")]
    public string Title { get; set; }
    
    [JsonPropertyName("short-title")]
    public string ShortTitle { get; set; }

    [JsonPropertyName("total-parts")]
    public int TotalParts { get; set; }
}