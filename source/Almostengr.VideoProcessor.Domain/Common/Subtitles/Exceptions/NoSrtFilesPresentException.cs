namespace Almostengr.VideoProcessor.Domain.Common.Subtitles.Exceptions;

public sealed class NoSrtFilesPresentException : VideoProcessorException
{
    public NoSrtFilesPresentException() : base()
    {
    }

    public NoSrtFilesPresentException(string message) : base(message)
    {
    }
}