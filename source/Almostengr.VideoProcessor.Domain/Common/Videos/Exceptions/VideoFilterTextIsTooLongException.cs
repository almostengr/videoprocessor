namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

internal class VideoFilterTextIsTooLongException : VideoProcessorException
{
    public VideoFilterTextIsTooLongException() : base()
    {
    }

    public VideoFilterTextIsTooLongException(string message) : base(message)
    {
    }

}