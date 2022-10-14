using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

[Serializable]
internal sealed class DiskSpaceIsLowException : VideoProcessorException
{
    public DiskSpaceIsLowException()
    {
    }

    public DiskSpaceIsLowException(string message) : base(message)
    {
    }

    public DiskSpaceIsLowException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public DiskSpaceIsLowException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}