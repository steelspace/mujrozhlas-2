using LiteDB;

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

    [BsonField("id")]
    public string Id { get; set; }

    [BsonField("episode-id")]
    public string EpisodeId { get; set; }

    [BsonField("playable-till")]
    public DateTimeOffset PlayableTill { get; set; }

    [BsonField("variant")]
    public string Variant { get; set; }

    [BsonField("duration-seconds")]
    public int DurationSeconds { get; set; }

    [BsonField("url")]
    public string Url { get; set; }
}