using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

internal sealed class TarballExtractingException : VideoProcessorException
{
    public TarballExtractingException()
    {
    }

    public TarballExtractingException(string message) : base(message)
    {
    }

    public TarballExtractingException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public TarballExtractingException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}