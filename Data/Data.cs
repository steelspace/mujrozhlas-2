using System.Text.Json.Serialization;

public class Data
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("relationships")]
    public Relationships? Relationships { get; set; }
}
