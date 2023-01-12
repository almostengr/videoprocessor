using Almostengr.VideoProcessor.Core.Common;

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