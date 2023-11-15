using LiteDB;

namespace Mujrozhlas.Data;
public class Episode
{
    public Episode(string id, string title, string shortTitle, int part, string serialId)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        Part = part;
        SerialId = serialId;
    }

    [BsonField("id")]
    public string Id { get; set; }

    [BsonField("serial-id")]
    public string SerialId { get; set; }

    [BsonField("title")]
    public string Title { get; set; }
    
    [BsonField("short-title")]
    public string ShortTitle { get; set; }

    [BsonField("part")]
    public int Part { get; set; }

    [BsonField("audio-links")]
    public List<AudioLink> AudioLinks  { get; set; } = new List<AudioLink>();
}