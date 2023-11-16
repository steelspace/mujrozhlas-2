using System.Text.Json.Serialization;

namespace Mujrozhlas.Data;
public class AudioLink
{
    public AudioLink(string id, string episodeId, DateTimeOffset playableTill, string variant, int durationSeconds, string url)
    {
        Id = id;
        EpisodeId = episodeId;
        PlayableTill = playableTill;
        Variant = variant;
        DurationSeconds = durationSeconds;
        Url = url;
    }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("episode-id")]
    public string EpisodeId { get; set; }

    [JsonPropertyName("playable-till")]
    public DateTimeOffset PlayableTill { get; set; }

    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    [JsonPropertyName("duration-seconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}