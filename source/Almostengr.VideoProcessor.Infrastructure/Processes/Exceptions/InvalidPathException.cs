using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class InvalidPathException : VideoProcessorException
{
    public InvalidPathException()
    {
    }

    public InvalidPathException(string message) : base(message)
    {
    }
}