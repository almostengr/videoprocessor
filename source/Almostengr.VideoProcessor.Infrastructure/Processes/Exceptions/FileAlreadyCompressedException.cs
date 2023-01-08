using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

internal class FileAlreadyCompressedException : VideoProcessorException
{
    public FileAlreadyCompressedException()
    {
    }

    public FileAlreadyCompressedException(string message) : base(message)
    {
    }
}