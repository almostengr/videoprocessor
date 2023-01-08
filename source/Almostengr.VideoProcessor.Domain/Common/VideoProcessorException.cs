namespace Almostengr.VideoProcessor.Domain.Common;

public class VideoProcessorException : Exception
{
    protected VideoProcessorException() : base()
    {
    }
    
    public VideoProcessorException(string message) : base(message)
    {
    }
}