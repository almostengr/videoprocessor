namespace Almostengr.VideoProcessor.Domain.Common.Subtitles.Exceptions;

public sealed class NoSubtitleFilesPresentException : VideoProcessorException
{
    public NoSubtitleFilesPresentException() : base()
    {
    }

    public NoSubtitleFilesPresentException(string message) : base(message)
    {
    }
}