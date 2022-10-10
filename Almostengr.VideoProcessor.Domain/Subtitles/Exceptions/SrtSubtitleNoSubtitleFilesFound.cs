using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Services.Exceptions;

[Serializable]
internal class SrtSubtitleNoSubtitleFilesFound : VideoProcessorException
{
    public SrtSubtitleNoSubtitleFilesFound()
    {
    }

    public SrtSubtitleNoSubtitleFilesFound(string? message) : base(message)
    {
    }

    public SrtSubtitleNoSubtitleFilesFound(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SrtSubtitleNoSubtitleFilesFound(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}