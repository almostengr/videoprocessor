using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

[Serializable]
internal sealed class VideoOutputFileNameIsNullOrWhiteSpace : Exception
{
    public VideoOutputFileNameIsNullOrWhiteSpace()
    {
    }

    public VideoOutputFileNameIsNullOrWhiteSpace(string message) : base(message)
    {
    }

    public VideoOutputFileNameIsNullOrWhiteSpace(string message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VideoOutputFileNameIsNullOrWhiteSpace(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}