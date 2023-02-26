using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

public sealed class SaveFilePathIsNullOrEmptyException : VideoProcessorException
{
    public SaveFilePathIsNullOrEmptyException()
    {
    }

    public SaveFilePathIsNullOrEmptyException(string message) : base(message)
    {
    }
}