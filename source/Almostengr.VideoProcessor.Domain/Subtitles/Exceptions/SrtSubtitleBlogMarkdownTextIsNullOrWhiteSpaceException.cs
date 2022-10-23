using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

internal sealed class SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException()
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}