using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

public sealed class SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException()
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }
}