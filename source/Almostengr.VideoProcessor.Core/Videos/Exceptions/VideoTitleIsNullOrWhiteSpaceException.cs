using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions;
[Serializable]
internal class VideoTitleIsNullOrWhiteSpaceException : Exception
{
    public VideoTitleIsNullOrWhiteSpaceException()
    {
    }

    public VideoTitleIsNullOrWhiteSpaceException(string? message) : base(message)
    {
    }

    public VideoTitleIsNullOrWhiteSpaceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VideoTitleIsNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}