using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

internal sealed class VideoTarballFilePathIsNullOrEmptyException : VideoProcessorException
{
    public VideoTarballFilePathIsNullOrEmptyException()
    {
    }

    public VideoTarballFilePathIsNullOrEmptyException(string message) : base(message)
    {
    }

    public VideoTarballFilePathIsNullOrEmptyException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public VideoTarballFilePathIsNullOrEmptyException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}