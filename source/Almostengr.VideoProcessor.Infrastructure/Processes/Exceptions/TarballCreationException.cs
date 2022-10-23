namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class TarballCreationException : Exception
{
    public TarballCreationException()
    {
    }

    public TarballCreationException(string? message) : base(message)
    {
    }
}