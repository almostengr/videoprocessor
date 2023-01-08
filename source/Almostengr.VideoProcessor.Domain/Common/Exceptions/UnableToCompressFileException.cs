namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public class UnableToCompressFileException : VideoProcessorException
{
    public UnableToCompressFileException()
    {
    }

    public UnableToCompressFileException(string message) : base(message)
    {
    }
}