using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

[Serializable]
internal sealed class SaveFilePathIsNullOrEmptyException : VideoProcessorException
{
    public SaveFilePathIsNullOrEmptyException()
    {
    }

    public SaveFilePathIsNullOrEmptyException(string message) : base(message)
    {
    }

    public SaveFilePathIsNullOrEmptyException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public SaveFilePathIsNullOrEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}