using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

internal sealed class FfmpegRenderVideoException : VideoProcessorException
{
    public FfmpegRenderVideoException()
    {
    }

    public FfmpegRenderVideoException(string message) : base(message)
    {
    }
}