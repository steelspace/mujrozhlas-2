using System.Text.RegularExpressions;

namespace Mujrozhlas.Downloader;

public class SanitizedFileName
{
    // https://msdn.microsoft.com/en-us/library/aa365247.aspx#naming_conventions
    // http://stackoverflow.com/questions/146134/how-to-remove-illegal-characters-from-path-and-filenames
    private static readonly Regex removeInvalidChars = new Regex($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]",
        RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public SanitizedFileName(string fileName, string replacement = "_")
    {
        Value = removeInvalidChars.Replace(fileName, replacement);
    }

    public string Value { get; }
}