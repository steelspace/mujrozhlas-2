using System.Text.Json.Serialization;

namespace Extractor.Common;
public class ParsedEpisode
{
    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("uuid")]
    public string Uuid { get; set; }

    public ParsedEpisode(string id, string uuid)
    {
        Id = id;
        Uuid = uuid;
    }
}