namespace Extractor.Common;
public class ExtractorException : ApplicationException
{
    public ExtractorException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}