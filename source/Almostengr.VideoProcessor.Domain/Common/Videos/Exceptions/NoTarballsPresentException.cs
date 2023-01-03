namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

public sealed class NoTarballsPresentException : VideoProcessorException
{
    public NoTarballsPresentException() : base()
    {
    }

    public NoTarballsPresentException(string message) : base(message)
    {
    }
}