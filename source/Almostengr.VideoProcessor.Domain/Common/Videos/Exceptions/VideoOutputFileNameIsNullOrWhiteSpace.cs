namespace Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

public sealed class VideoOutputFileNameIsNullOrWhiteSpace : Exception
{
    public VideoOutputFileNameIsNullOrWhiteSpace() : base()
    {
    }

    public VideoOutputFileNameIsNullOrWhiteSpace(string message) : base(message)
    {
    }
}