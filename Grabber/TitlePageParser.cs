
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using MujRozhlas.Data;
using MujRozhlas.Common;
using HtmlAgilityPack;
using MujRozhlas.Database;

namespace Extractor;
public class TitlePageParser
{
    private readonly IDatabase database;

    public TitlePageParser(IDatabase database)
    {
        this.database = database;
    }

    public Serial? GetSerial(ParsedEpisode parsedEpisode)
    {
        if (parsedEpisode?.Id is null)
        {
            throw new ExtractorException("Episode UUID is missing");
        }

        string? episodeResponse = LoadHtmlContent($"https://api.mujrozhlas.cz/episodes/{parsedEpisode.Uuid}");

        if (episodeResponse is null)
        {
            throw new ExtractorException("Episode is missing", null);
        }

        string? serialId = null;

        try
        {
            var episodeNode = JsonNode.Parse(episodeResponse);

            if (episodeNode is null)
            {
                throw new ExtractorException("Missing 'Episode'", null);
            }

            if (episodeNode["data"]!["type"]!.GetValue<string>() != "episode")
            {
                throw new ExtractorException("Wrong type of 'Episode'", null);
            }

            var serialNode = episodeNode["data"]!["relationships"]!["serial"];

            if (serialNode?["data"]?["id"] is not null)
            {
                serialId = serialNode!["data"]!["id"]!.GetValue<string>();
            }
        }

        catch (Exception ex)
        {
            throw new ExtractorException("Error in 'Episode'", ex);
        }

        if (serialId is not null)
        {
            string? serialResponse = LoadHtmlContent($"https://api.mujrozhlas.cz/serials/{serialId}");

            if (serialResponse is null)
            {
                throw new ExtractorException("Serial is not parsed", null);
            }

            string title;
            string shortTitle;
            int totalParts;
            string coverArtUrl;

            try
            {
                var serialNode = JsonNode.Parse(serialResponse);

                if (serialNode is null)
                {
                    throw new ExtractorException("Serial is missing", null);
                }

                if (serialNode["data"]!["type"]!.GetValue<string>() != "serial")
                {
                    throw new ExtractorException("Wrong type of 'Serial'", null);
                }

                title = serialNode["data"]!["attributes"]!["title"]!.GetValue<string>();
                shortTitle = serialNode["data"]!["attributes"]!["shortTitle"]!.GetValue<string>();
                totalParts = serialNode["data"]!["attributes"]!["totalParts"]!.GetValue<int>();
                coverArtUrl = serialNode["data"]!["attributes"]!["asset"]!["url"]!.GetValue<string>();
            }

            catch (Exception ex)
            {
                throw new ExtractorException("Error in 'Episode'", ex);
            }

            var serial = new Serial(serialId, title, shortTitle, totalParts, coverArtUrl, DateTimeOffset.Now);

            return serial;
        }
        else
        {
            return null;
        }
    }

    public ParsedEpisode ExtractTitleInformation(string url)
    {
        // Load HTML content from the URL
        string htmlContent = LoadHtmlContent(url);
        // Load HTML string into HtmlDocument
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Select nodes with class "b-episode"
        var episodeNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'b-episode')]");
        var episodeNode = episodeNodes?.FirstOrDefault();

        if (episodeNode is null)
        {
            throw new ExtractorException("No episodes found with class 'b-episode'");

        }

        Console.WriteLine($"Episodes found, parsing continues: {url}");

        // Get the data-entry attribute value
        string dataEntryValue = episodeNode.GetAttributeValue("data-entry", string.Empty);

        var decodedHtml = WebUtility.HtmlDecode(dataEntryValue);

        var episode = JsonSerializer.Deserialize<ParsedEpisode>(decodedHtml);

        if (episode is null)
        {
            throw new ExtractorException("Episode is not parsed");
        }

        Console.WriteLine($"Episode {episode.Uuid} found.");

        return episode;
    }

    public ParsedEpisode? ExtractTitleInformationFromPlayerWrapper(string url)
    {
        // Load HTML content from the URL
        string htmlContent = LoadHtmlContent(url);
        // Load HTML string into HtmlDocument
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // <section class="player-wrapper" data-entry=
        // not a series show
        var episodeNodes = doc.DocumentNode.SelectNodes("//section[contains(@class, 'player-wrapper')]");
        var episodeNode = episodeNodes?.FirstOrDefault();

        if (episodeNode is null)
        {
            return null;
        }

        Console.WriteLine($"Episodes found, parsing continues: {url}");

        // Get the data-entry attribute value
        string dataEntryValue = episodeNode.GetAttributeValue("data-entry", string.Empty);

        var decodedHtml = WebUtility.HtmlDecode(dataEntryValue);

        var episode = JsonSerializer.Deserialize<ParsedEpisode>(decodedHtml);

        if (episode is null)
        {
            throw new ExtractorException("Episode is not parsed");
        }

        Console.WriteLine($"Episode {episode.Uuid} found.");

        return episode;
    }

    public List<Episode> GetAvailableEpisodes(string serialId)
    {
        var serial = database.GetSerial(serialId);

        if (serial is not null && serial.IsNonSerial)
        {
            // there is only 1 part, no need for refresh
            var episodes = database.GetEpisodes(serialId);
            return episodes;
        }

        string? serialEpisodesResponse = LoadHtmlContent($"https://api.mujrozhlas.cz/serials/{serialId}/episodes");

        if (serialEpisodesResponse is null)
        {
            throw new ExtractorException("Episodes are missing", null);
        }

        try
        {
            var episodesJsonNode = JsonNode.Parse(serialEpisodesResponse);
            var episodeNodes = episodesJsonNode!["data"]!.AsArray();
            var episodes = new List<Episode>();

            foreach (var episodeNode in episodeNodes)
            {
                var episode = GetEpisode(serialId, episodeNode);
                if (episode is not null)
                {
                    episodes.Add(episode);
                }
            }

            return episodes;
        }

        catch (Exception ex)
        {
            throw new ExtractorException("Error in 'Episode'", ex);
        }
    }

    public Episode GetNonSerialEpisode(string episodeId)
    {
        string? episodeResponse = LoadHtmlContent($"https://api.mujrozhlas.cz/episodes/{episodeId}");

        if (episodeResponse is null)
        {
            throw new ExtractorException("Non-serial episode is not parsed", null);
        }

        try
        {
            var episodeNode = JsonNode.Parse(episodeResponse);
            var episode = GetEpisode(episodeId, episodeNode!["data"]);

            if (episode is null)
            {
                throw new ExtractorException("Non-serial episode is not found", null);
            }            

            return episode;
        }

        catch (Exception ex)
        {
            throw new ExtractorException("Error in 'Episode'", ex);
        }        
    } 

    Episode? GetEpisode(string serialId, JsonNode? episodeNode)
    {
        string episodeId = string.Empty;
        string title = string.Empty;
        string shortTitle = string.Empty;
        string coverArtUrl = string.Empty;
        DateTimeOffset since = DateTimeOffset.Now;
        DateTimeOffset till = DateTimeOffset.Now;
        DateTimeOffset updated = DateTimeOffset.Now;
        int part = 0;

        if (episodeNode is not null &&
             episodeNode["type"]!.GetValue<string>() == "episode")
        {
            episodeId = episodeNode["id"]!.GetValue<string>();
            title = episodeNode["attributes"]!["title"]!.GetValue<string>();
            shortTitle = episodeNode["attributes"]!["shortTitle"]!.GetValue<string>();
            int? parsedPart = episodeNode["attributes"]?["part"]?.GetValue<int?>();
            part = parsedPart ?? 1;

            since = episodeNode["attributes"]!["since"]!.GetValue<DateTimeOffset>();
            till = episodeNode["attributes"]!["till"]!.GetValue<DateTimeOffset>();
            updated = episodeNode["attributes"]!["updated"]!.GetValue<DateTimeOffset>();
            coverArtUrl = episodeNode["attributes"]!["asset"]!["url"]!.GetValue<string>();

            var audioLinks = episodeNode["attributes"]!["audioLinks"]!.AsArray();

            var episode = new Episode(episodeId, title, shortTitle, part, serialId, since, till, updated, coverArtUrl);

            foreach (var audioLink in audioLinks)
            {
                string variant = audioLink!["variant"]!.GetValue<string>();

                var playAbleTill = audioLink!["playableTill"]?.GetValue<DateTimeOffset>();

                var audio = new AudioLink(
                    $"{episodeId}/{variant}",
                    episodeId,
                    playAbleTill ?? DateTimeOffset.MaxValue,
                    variant,
                    audioLink!["duration"]!.GetValue<int>(),
                    audioLink!["url"]!.GetValue<string>()
                );

                episode.AudioLinks.Add(audio);
            }

            return episode;
        }

        return null;
    }

    static string LoadHtmlContent(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Download the HTML content from the specified URL
                return client.GetStringAsync(url).Result;
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error loading HTML content: {e.Message}");
                return string.Empty;
            }
        }
    }
}
