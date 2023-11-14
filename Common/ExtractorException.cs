namespace Mujrozhlas.Common;
public class ExtractorException : ApplicationException
{
    public ExtractorException(string? message, Exception? innerException = null) : base(message, innerException)
    {
    }
}