namespace Almostengr.VideoProcessor.Domain.Videos.Exceptions;

public sealed class VideoOutputFileNameIsNullOrWhiteSpace : Exception
{
    public VideoOutputFileNameIsNullOrWhiteSpace()
    {
    }

    public VideoOutputFileNameIsNullOrWhiteSpace(string message) : base(message)
    {
    }
}