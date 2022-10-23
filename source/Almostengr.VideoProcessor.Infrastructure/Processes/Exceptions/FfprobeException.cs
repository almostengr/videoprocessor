using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

internal sealed class FfprobeException : VideoProcessorException
{
    public FfprobeException()
    {
    }

    public FfprobeException(string message) : base(message)
    {
    }
}