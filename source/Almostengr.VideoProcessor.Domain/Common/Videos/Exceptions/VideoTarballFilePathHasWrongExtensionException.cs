
namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

public sealed class VideoTarballFilePathHasWrongExtensionException : Exception
{
    public VideoTarballFilePathHasWrongExtensionException() : base()
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string message) : base(message)
    {
    }
}