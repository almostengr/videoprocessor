namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

public class VideoProcessorException : Exception
{
    public VideoProcessorException(string? message) : base(message)
    {
    }
}