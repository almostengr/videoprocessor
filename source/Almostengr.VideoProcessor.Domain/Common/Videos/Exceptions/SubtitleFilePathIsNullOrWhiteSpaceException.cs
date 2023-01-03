namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

internal class SubtitleFilePathIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SubtitleFilePathIsNullOrWhiteSpaceException() : base()
    {
    }

    public SubtitleFilePathIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}