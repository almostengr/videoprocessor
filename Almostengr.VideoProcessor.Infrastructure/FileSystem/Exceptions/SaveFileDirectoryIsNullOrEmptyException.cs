using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

[Serializable]
internal sealed class SaveFileDirectoryIsNullOrEmptyException : VideoProcessorException
{
    public SaveFileDirectoryIsNullOrEmptyException()
    {
    }

    public SaveFileDirectoryIsNullOrEmptyException(string message) : base(message)
    {
    }

    public SaveFileDirectoryIsNullOrEmptyException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public SaveFileDirectoryIsNullOrEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}