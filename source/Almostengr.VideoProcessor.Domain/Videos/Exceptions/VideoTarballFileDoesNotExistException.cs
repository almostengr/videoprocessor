using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

public sealed class VideoTarballFileDoesNotExistException : VideoProcessorException
{
    public VideoTarballFileDoesNotExistException()
    {
    }

    public VideoTarballFileDoesNotExistException(string message) : base(message)
    {
    }
}