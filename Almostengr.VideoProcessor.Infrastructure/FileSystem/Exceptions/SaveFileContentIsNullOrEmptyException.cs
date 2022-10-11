using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;
 
[Serializable]
internal sealed class SaveFileContentIsNullOrEmptyException : VideoProcessorException
{
    public SaveFileContentIsNullOrEmptyException()
    {
    }

    public SaveFileContentIsNullOrEmptyException(string message) : base(message)
    {
    }

    public SaveFileContentIsNullOrEmptyException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public SaveFileContentIsNullOrEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}