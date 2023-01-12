
namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

internal class InvalidSubtitleFileException : VideoProcessorException
{
    public InvalidSubtitleFileException() : base()
    {
    }

    public InvalidSubtitleFileException(string message) : base(message)
    {
    }
}
