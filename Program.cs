using System.Diagnostics;

var parser = new Extractor.TitlePageParser();

var episodes = await parser.ExtractTitleInformation();
Console.WriteLine(episodes);