
namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

public sealed class NoSubtitleFilesPresentException : VideoProcessorException
{
    public NoSubtitleFilesPresentException() : base()
    {
    }

    public NoSubtitleFilesPresentException(string message) : base(message)
    {
    }
}