namespace Almostengr.VideoProcessor.Domain.Common.Subtitles;

internal class InvalidSubtitleFileException : VideoProcessorException
{
    public InvalidSubtitleFileException() : base()
    {
    }

    public InvalidSubtitleFileException(string message) : base(message)
    {
    }
}
