namespace Almostengr.VideoProcessor.Domain.Common;

public abstract class VideoProcessorException : Exception
{
    protected VideoProcessorException() : base()
    {
    }
    
    public VideoProcessorException(string message) : base(message)
    {
    }
}