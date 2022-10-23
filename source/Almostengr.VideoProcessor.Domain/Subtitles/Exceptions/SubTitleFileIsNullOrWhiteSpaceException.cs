using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

internal sealed class SubTitleFileIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SubTitleFileIsNullOrWhiteSpaceException()
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}