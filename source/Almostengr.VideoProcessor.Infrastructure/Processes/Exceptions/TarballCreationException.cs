namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

internal class TarballCreationException : Exception
{
    public TarballCreationException()
    {
    }

    public TarballCreationException(string? message) : base(message)
    {
    }
}