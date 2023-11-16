
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using MujRozhlas.Data;
using MujRozhlas.Common;
using HtmlAgilityPack;

namespace Extractor;
public class TitlePageParser
{
    public async Task<IEnumerable<ParsedEpisode>> ExtractTitleInformation(string url)
    {
        var episodes = new List<ParsedEpisode>();

        // Load HTML content from the URL
        string htmlContent = await LoadHtmlContent(url);
        // Load HTML string into HtmlDocument
        var doc = new HtmlDocument();
        doc.LoadHtml(htmlContent);

        // Select nodes with class "b-episode"
        HtmlNodeCollection episodeNodes = doc.DocumentNode.SelectNodes("//article[contains(@class, 'b-episode')]");

        if (episodeNodes != null)
        {
            Console.WriteLine($"Episodes found, parsing continues: {url}");

            foreach (HtmlNode episodeNode in episodeNodes)
            {
                // Get the data-entry attribute value
                string dataEntryValue = episodeNode.GetAttributeValue("data-entry", string.Empty);

                var decodedHtml = WebUtility.HtmlDecode(dataEntryValue);

                var episode = JsonSerializer.Deserialize<ParsedEpisode>(decodedHtml);

                if (episode is null)
                {
                    throw new ExtractorException("Episode is not parsed");
                }
                
                episodes.Add(episode);
                Console.WriteLine($"Episode {episode.Uuid} found.");
            }

            return episodes;
        }
        else
        {
            throw new ExtractorException("No episodes found with class 'b-episode'");
        }
    }

    public async Task<Serial> GetSerial(IEnumerable<ParsedEpisode> parsedEpisodes)
    {
        var parsedEpisode = parsedEpisodes.FirstOrDefault();

        if (parsedEpisode?.Id is null)
        {
            throw new ExtractorException("Episode UUID is missing");
        }

        string? episodeResponse = await LoadHtmlContent($"https://api.mujrozhlas.cz/episodes/{parsedEpisode.Uuid}");

        if (episodeResponse is null)
        {
            throw new ExtractorException("Episode is missing", null);
        }

        string serialId;
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

            serialId = episodeNode["data"]!["relationships"]!["serial"]!["data"]!["id"]!.GetValue<string>();
        }

        catch (Exception ex)
        {
            throw new ExtractorException("Error in 'Episode'", ex);
        }

        string? serialResponse = await LoadHtmlContent($"https://api.mujrozhlas.cz/serials/{serialId}");

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

    public async Task<List<Episode>> GetAvailableEpisodes(string serialId)
    {
        string? serialEpisodesResponse = await LoadHtmlContent($"https://api.mujrozhlas.cz/serials/{serialId}/episodes");

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
                string episodeId = string.Empty;
                string title = string.Empty;
                string shortTitle = string.Empty;
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
                    part = episodeNode["attributes"]!["part"]!.GetValue<int>();

                    since = episodeNode["attributes"]!["since"]!.GetValue<DateTimeOffset>();
                    till = episodeNode["attributes"]!["till"]!.GetValue<DateTimeOffset>();
                    updated = episodeNode["attributes"]!["updated"]!.GetValue<DateTimeOffset>();

                    var audioLinks = episodeNode["attributes"]!["audioLinks"]!.AsArray();

                    var episode = new Episode(episodeId, title, shortTitle, part, serialId, since, till, updated);

                    foreach (var audioLink in audioLinks)
                    {
                        string variant = audioLink!["variant"]!.GetValue<string>();

                        var audio = new AudioLink(
                            $"{episodeId}/{variant}",
                            episodeId,
                            audioLink!["playableTill"]!.GetValue<DateTimeOffset>(),
                            variant,
                            audioLink!["duration"]!.GetValue<int>(),
                            audioLink!["url"]!.GetValue<string>()
                        );

                        episode.AudioLinks.Add(audio);
                    }

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
   
    static async Task<string> LoadHtmlContent(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Download the HTML content from the specified URL
                return await client.GetStringAsync(url);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error loading HTML content: {e.Message}");
                return string.Empty;
            }
        }
    }
}
