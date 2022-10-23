
namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

public sealed class VideoTarballFilePathHasWrongExtensionException : Exception
{
    public VideoTarballFilePathHasWrongExtensionException()
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string message) : base(message)
    {
    }
}