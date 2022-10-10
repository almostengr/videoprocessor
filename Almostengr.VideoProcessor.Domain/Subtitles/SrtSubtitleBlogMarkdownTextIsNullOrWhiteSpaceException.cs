using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal class SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException : Exception
{
    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException()
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(string? message) : base(message)
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}