namespace Almostengr.VideoProcessor.Core.Common.Exceptions;

public class UnableToCompressFileException : VideoProcessorException
{
    public UnableToCompressFileException()
    {
    }

    public UnableToCompressFileException(string message) : base(message)
    {
    }
}