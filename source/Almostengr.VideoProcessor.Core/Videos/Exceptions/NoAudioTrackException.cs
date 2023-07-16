using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions;

[Serializable]
internal class NoAudioTrackException : VideoProcessorException
{
    public NoAudioTrackException()
    {
    }

    public NoAudioTrackException(string? message) : base(message)
    {
    }

    public NoAudioTrackException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoAudioTrackException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}