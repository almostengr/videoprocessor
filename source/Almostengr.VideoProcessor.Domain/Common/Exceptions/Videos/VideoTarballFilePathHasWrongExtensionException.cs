
namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public sealed class VideoTarballFilePathHasWrongExtensionException : Exception
{
    public VideoTarballFilePathHasWrongExtensionException()
    {
    }

    public VideoTarballFilePathHasWrongExtensionException(string message) : base(message)
    {
    }
}