using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

public sealed class TarballExtractingException : VideoProcessorException
{
    public TarballExtractingException()
    {
    }

    public TarballExtractingException(string message) : base(message)
    {
    }
}