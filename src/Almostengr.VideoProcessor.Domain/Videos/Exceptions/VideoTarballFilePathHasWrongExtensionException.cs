using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

[Serializable]
internal sealed class VideoTarballFilePathHasWrongExtensionException : Exception
{
    public VideoTarballFilePathHasWrongExtensionException()
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string message) : base(message)
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}