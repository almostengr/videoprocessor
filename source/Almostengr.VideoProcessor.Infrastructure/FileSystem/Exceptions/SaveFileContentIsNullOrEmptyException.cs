using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;
 
internal sealed class SaveFileContentIsNullOrEmptyException : VideoProcessorException
{
    public SaveFileContentIsNullOrEmptyException()
    {
    }

    public SaveFileContentIsNullOrEmptyException(string message) : base(message)
    {
    }
}