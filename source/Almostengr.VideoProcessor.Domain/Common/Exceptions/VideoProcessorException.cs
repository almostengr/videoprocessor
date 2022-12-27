namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public abstract class VideoProcessorException : Exception
{
    protected VideoProcessorException()
    {
    }
    
    public VideoProcessorException(string message) : base(message)
    {
    }
}