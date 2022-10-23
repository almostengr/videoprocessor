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
}