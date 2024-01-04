using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

public sealed class DiskSpaceIsLowException : VideoProcessorException
{
    public DiskSpaceIsLowException(string message) : base(message)
    {
    }
}