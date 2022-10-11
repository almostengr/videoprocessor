using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

internal sealed class FfmpegRenderVideoException : VideoProcessorException
{
    public FfmpegRenderVideoException()
    {
    }

    public FfmpegRenderVideoException(string message) : base(message)
    {
    }

    public FfmpegRenderVideoException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public FfmpegRenderVideoException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}