namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public sealed class NoTarballsPresentException : VideoProcessorException
{
    public NoTarballsPresentException()
    {
    }

    public NoTarballsPresentException(string message) : base(message)
    {
    }
}