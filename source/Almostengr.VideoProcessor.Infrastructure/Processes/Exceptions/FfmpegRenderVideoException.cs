using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class FfmpegRenderVideoException : VideoProcessorException
{
    public FfmpegRenderVideoException()
    {
    }

    public FfmpegRenderVideoException(string message) : base(message)
    {
    }
}