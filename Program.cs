using System.Diagnostics;

var parser = new Extractor.TitlePageParser();

var episodes = await parser.ExtractTitleInformation();
await parser.GetEpisodes(episodes);
Console.WriteLine(episodes);