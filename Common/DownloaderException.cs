namespace Mujrozhlas.Common;
public class DownloadedException : ApplicationException
{
    public DownloadedException(string? message, Exception? innerException = null) : base(message, innerException)
    {
    }
}