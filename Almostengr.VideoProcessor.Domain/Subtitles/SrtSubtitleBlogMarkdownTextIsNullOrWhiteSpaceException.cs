using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal sealed class SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException()
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}