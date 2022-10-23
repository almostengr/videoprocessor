namespace Almostengr.VideoProcessor.Domain.Common;

public abstract class VideoProcessorException : Exception
{
    protected VideoProcessorException()
    {
    }
    
    public VideoProcessorException(string message) : base(message)
    {
    }
}