namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public sealed class UnableToDecompressFileException : VideoProcessorException
{
    public UnableToDecompressFileException()
    {
    }

    public UnableToDecompressFileException(string message) : base(message)
    {
    }
}