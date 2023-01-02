namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

public sealed class VideoTarballFilePathIsNullOrEmptyException : VideoProcessorException
{
    public VideoTarballFilePathIsNullOrEmptyException()
    {
    }

    public VideoTarballFilePathIsNullOrEmptyException(string message) : base(message)
    {
    }
}