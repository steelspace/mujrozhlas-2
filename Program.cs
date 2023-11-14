using System.Diagnostics;

var parser = new Extractor.TitlePageParser();

var parsedEpisodes = await parser.ExtractTitleInformation();
var serial = await parser.GetSerial(parsedEpisodes);
var episodes = await parser.GetAvailableEpisodes(serial.Id);

