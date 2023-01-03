namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

public sealed class VideoTarballFilePathIsNullOrEmptyException : VideoProcessorException
{
    public VideoTarballFilePathIsNullOrEmptyException() : base()
    {
    }

    public VideoTarballFilePathIsNullOrEmptyException(string message) : base(message)
    {
    }
}