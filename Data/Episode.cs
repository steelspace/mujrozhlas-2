using System.Text.Json.Serialization;

namespace Extractor.Common;
public record Episode()
{
    public string id { get; set; }
    public string uuid { get; set; }
    public string type { get; set; }
    public string title { get; set; }
    public string url { get; set; }

    [JsonPropertyName("show-id")]
    public string showId { get; set; }

    [JsonPropertyName("show-uuid")]
    public string showUuid { get; set; }
};