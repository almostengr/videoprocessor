using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

public sealed class SaveFileDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SaveFileDirectoryIsNullOrEmptyException()
    {
    }

    public SaveFileDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }
}