using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

internal sealed class ProgramWorkingDirectoryIsInvalidException : VideoProcessorException
{
    public ProgramWorkingDirectoryIsInvalidException()
    {
    }

    public ProgramWorkingDirectoryIsInvalidException(string message) : base(message)
    {
    }
}