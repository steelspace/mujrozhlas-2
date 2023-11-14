using Mujrozhlas.Database;

IDatabase database = new LiteDbDatabase();

var parser = new Extractor.TitlePageParser();
var parsedEpisodes = await parser.ExtractTitleInformation();

var serial = await parser.GetSerial(parsedEpisodes);
database.SaveSerial(serial);

var episodes = await parser.GetAvailableEpisodes(serial.Id);
database.SaveEpisodes(episodes);

var t = database.LoadSerial(serial.Id);
Console.WriteLine(t);
