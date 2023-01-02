using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

public sealed class DiskSpaceIsLowException : VideoProcessorException
{
    public DiskSpaceIsLowException()
    {
    }

    public DiskSpaceIsLowException(string message) : base(message)
    {
    }
}