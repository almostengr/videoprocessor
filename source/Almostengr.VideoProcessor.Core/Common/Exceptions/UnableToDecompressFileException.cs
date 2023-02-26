namespace Almostengr.VideoProcessor.Core.Common.Exceptions;

public sealed class UnableToDecompressFileException : VideoProcessorException
{
    public UnableToDecompressFileException()
    {
    }

    public UnableToDecompressFileException(string message) : base(message)
    {
    }
}