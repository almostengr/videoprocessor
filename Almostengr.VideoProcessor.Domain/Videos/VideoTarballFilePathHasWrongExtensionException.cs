using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Videos;

[Serializable]
internal class VideoTarballFilePathHasWrongExtensionException : Exception
{
    public VideoTarballFilePathHasWrongExtensionException()
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string? message) : base(message)
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected VideoTarballFilePathHasWrongExtensionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}