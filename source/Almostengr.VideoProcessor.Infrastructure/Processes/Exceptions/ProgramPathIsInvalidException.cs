using System.Runtime.Serialization;
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

    public ProgramPathIsInvalidException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public ProgramPathIsInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}