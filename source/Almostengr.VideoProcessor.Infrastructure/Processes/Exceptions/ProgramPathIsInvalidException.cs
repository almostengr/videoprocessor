using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class ProgramPathIsInvalidException : VideoProcessorException
{
    public ProgramPathIsInvalidException()
    {
    }

    public ProgramPathIsInvalidException(string message) : base(message)
    {
    }
}