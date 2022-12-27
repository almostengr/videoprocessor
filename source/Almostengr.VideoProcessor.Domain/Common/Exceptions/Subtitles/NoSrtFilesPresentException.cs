namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public sealed class NoSrtFilesPresentException : VideoProcessorException
{
    public NoSrtFilesPresentException()
    {
    }

    public NoSrtFilesPresentException(string message) : base(message)
    {
    }
}