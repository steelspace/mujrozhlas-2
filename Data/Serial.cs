using LiteDB;

namespace Mujrozhlas.Data;
public class Serial
{
    [BsonField("id")]
    public string Id { get; set; }

    public Serial(string id, string title, string shortTitle, int totalParts)
    {
        Id = id;
        Title = title;
        ShortTitle = shortTitle;
        TotalParts = totalParts;
    }

    [BsonField("title")]
    public string Title { get; set; }
    
    [BsonField("short-title")]
    public string ShortTitle { get; set; }

    [BsonField("total-parts")]
    public int TotalParts { get; set; }
}