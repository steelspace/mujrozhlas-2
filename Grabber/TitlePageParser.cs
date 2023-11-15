
using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using Mujrozhlas.Data;
using Mujrozhlas.Common;
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
            foreach (HtmlNode episodeNode in episodeNodes)
            {
                // Get the data-entry attribute value
                string dataEntryValue = episodeNode.GetAttributeValue("data-entry", string.Empty);

                var decodedHtml = WebUtility.HtmlDecode(dataEntryValue);
                Console.WriteLine($"Data-Entry Value: {decodedHtml}");

                var episode = JsonSerializer.Deserialize<ParsedEpisode>(decodedHtml);

                if (episode is null)
                {
                    throw new ExtractorException("Episode is not parsed");
                }
                
                episodes.Add(episode);
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

        string serialId = String.Empty;

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

        string title = String.Empty;
        string shortTitle = String.Empty;
        int totalParts = 0;

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
        }

        catch (Exception ex)
        {
            throw new ExtractorException("Error in 'Episode'", ex);
        }

        var serial = new Serial(serialId, title, shortTitle, totalParts);

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
                string episodeId = String.Empty;
                string title = String.Empty;
                string shortTitle = String.Empty;
                int part = 0;

                if (episodeNode is not null &&
                     episodeNode["type"]!.GetValue<string>() == "episode")
                {
                    episodeId = episodeNode["id"]!.GetValue<string>();
                    title = episodeNode["attributes"]!["title"]!.GetValue<string>();
                    shortTitle = episodeNode["attributes"]!["shortTitle"]!.GetValue<string>();
                    part = episodeNode["attributes"]!["part"]!.GetValue<int>();

                    var audioLinks = episodeNode["attributes"]!["audioLinks"]!.AsArray();

                    var episode = new Episode(title, shortTitle, serialId, part, serialId);

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
                return String.Empty;
            }
        }
    }
}
