using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class TarballCreationException : VideoProcessorException
{
    public TarballCreationException()
    {
    }

    public TarballCreationException(string? message) : base(message)
    {
    }
}