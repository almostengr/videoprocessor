using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

public sealed class VideoTarballFilePathIsNullOrEmptyException : VideoProcessorException
{
    public VideoTarballFilePathIsNullOrEmptyException()
    {
    }

    public VideoTarballFilePathIsNullOrEmptyException(string message) : base(message)
    {
    }
}