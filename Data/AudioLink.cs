using System.Text.Json.Serialization;

namespace Data;
public class AudioLink
{
    public AudioLink(DateTimeOffset playableTill, string variant, int durationSeconds, string url)
    {
        PlayableTill = playableTill;
        Variant = variant;
        DurationSeconds = durationSeconds;
        Url = url;
    }

    [JsonPropertyName("playable-till")]
    public DateTimeOffset PlayableTill { get; set; }

    [JsonPropertyName("variant")]
    public string Variant { get; set; }

    [JsonPropertyName("duration-seconds")]
    public int DurationSeconds { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}