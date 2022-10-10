using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed class VideoTarballFileDoesNotExistException : VideoProcessorException
{
    public VideoTarballFileDoesNotExistException()
    {
    }

    public VideoTarballFileDoesNotExistException(string message) : base(message)
    {
    }

    public VideoTarballFileDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public VideoTarballFileDoesNotExistException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}