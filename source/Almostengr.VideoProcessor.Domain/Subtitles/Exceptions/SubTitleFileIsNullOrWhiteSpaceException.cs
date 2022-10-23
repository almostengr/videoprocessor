using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

public sealed class SubTitleFileIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SubTitleFileIsNullOrWhiteSpaceException()
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}