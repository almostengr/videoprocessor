using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class FfprobeException : VideoProcessorException
{
    public FfprobeException()
    {
    }

    public FfprobeException(string message) : base(message)
    {
    }
}