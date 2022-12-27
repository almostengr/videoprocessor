namespace Almostengr.VideoProcessor.Domain.Common.Exceptions;

public sealed class VideoOutputFileNameIsNullOrWhiteSpace : Exception
{
    public VideoOutputFileNameIsNullOrWhiteSpace()
    {
    }

    public VideoOutputFileNameIsNullOrWhiteSpace(string message) : base(message)
    {
    }
}