using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class ProgramWorkingDirectoryIsInvalidException : VideoProcessorException
{
    public ProgramWorkingDirectoryIsInvalidException()
    {
    }

    public ProgramWorkingDirectoryIsInvalidException(string message) : base(message)
    {
    }
}