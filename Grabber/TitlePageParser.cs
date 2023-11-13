namespace Extractor;

using System.Dynamic;
using System.Net;
using System.Text.Json;
using Extractor.Common;
using HtmlAgilityPack;

public class TitlePageParser
{
    public async Task<IEnumerable<ParsedEpisode>> ExtractTitleInformation()
    {
        var episodes = new List<ParsedEpisode>();

        string url = "https://www.mujrozhlas.cz/podvecerni-cteni/alena-mornstajnova-listopad-co-kdyby-v-listopadu-1989-dopadlo-jinak";

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
                episodes.Add(episode);
            }

            return episodes;
        }
        else
        {
            throw new ExtractorException("No episodes found with class 'b-episode'", null);
        }
    }

    public async Task GetEpisodes(IEnumerable<ParsedEpisode> parsedEpisodes)
    {
        var parsedEpisode = parsedEpisodes.First();

        if (parsedEpisode.Id is null)
        {
            throw new ExtractorException("Episode UUID is missing", null);
        }

        string? episodeResponse = await LoadHtmlContent($"https://api.mujrozhlas.cz/episodes/{parsedEpisode.Uuid}");

        if (episodeResponse is null)
        {
            throw new ExtractorException("Episode is missing", null);
        }

        var episode = JsonSerializer.Deserialize<Episode>(episodeResponse);

        if (episode.data.Type != "episode")
        {
            throw new ExtractorException("Wrong type of 'Episode'", null);
        }

        if (episode.data.Relationships is null)
        {
            throw new ExtractorException("Relationships is missing in 'Episode'", null);
        }

        if (episode.data.Relationships.serial is null)
        {
            throw new ExtractorException("Serial is missing in 'Relationships'", null);
        }

        if (episode.data.Relationships.serial.data is null)
        {
            throw new ExtractorException("Data is missing in 'Serial'", null);
        }

        var serialId = episode.data.Relationships.serial.data.id;
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
                return null;
            }
        }
    }
}
