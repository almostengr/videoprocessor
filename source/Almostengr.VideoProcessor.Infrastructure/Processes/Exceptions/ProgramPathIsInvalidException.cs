using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

[Serializable]
internal sealed class ProgramPathIsInvalidException : VideoProcessorException
{
    public ProgramPathIsInvalidException()
    {
    }

    public ProgramPathIsInvalidException(string message) : base(message)
    {
    }
}