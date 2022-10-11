using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

internal sealed class FfprobeException : VideoProcessorException
{
    public FfprobeException()
    {
    }

    public FfprobeException(string message) : base(message)
    {
    }

    public FfprobeException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public FfprobeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}