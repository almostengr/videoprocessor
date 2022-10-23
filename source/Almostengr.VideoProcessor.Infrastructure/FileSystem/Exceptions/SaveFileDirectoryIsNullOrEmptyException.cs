using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

internal sealed class SaveFileDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SaveFileDirectoryIsNullOrEmptyException()
    {
    }

    public SaveFileDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }
}