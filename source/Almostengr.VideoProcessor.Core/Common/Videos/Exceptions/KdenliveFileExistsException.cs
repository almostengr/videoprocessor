using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

[Serializable]
public class KdenliveFileExistsException : VideoProcessorException
{
    public KdenliveFileExistsException()
    {
    }

    public KdenliveFileExistsException(string? message = "Archive has Kdenlive project file") : base(message)
    {
    }

    public KdenliveFileExistsException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected KdenliveFileExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}