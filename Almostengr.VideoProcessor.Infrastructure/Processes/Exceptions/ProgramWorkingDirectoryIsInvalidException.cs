using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

[Serializable]
internal sealed class ProgramWorkingDirectoryIsInvalidException : VideoProcessorException
{
    public ProgramWorkingDirectoryIsInvalidException()
    {
    }

    public ProgramWorkingDirectoryIsInvalidException(string message) : base(message)
    {
    }

    public ProgramWorkingDirectoryIsInvalidException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public ProgramWorkingDirectoryIsInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}