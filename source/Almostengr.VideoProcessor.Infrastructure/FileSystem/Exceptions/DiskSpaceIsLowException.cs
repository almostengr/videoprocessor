using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

internal sealed class DiskSpaceIsLowException : VideoProcessorException
{
    public DiskSpaceIsLowException()
    {
    }

    public DiskSpaceIsLowException(string message) : base(message)
    {
    }
}