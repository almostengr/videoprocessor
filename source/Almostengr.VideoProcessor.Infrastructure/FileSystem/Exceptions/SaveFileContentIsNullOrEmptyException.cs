using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;
 
public sealed class SaveFileContentIsNullOrEmptyException : VideoProcessorException
{
    public SaveFileContentIsNullOrEmptyException()
    {
    }

    public SaveFileContentIsNullOrEmptyException(string message) : base(message)
    {
    }
}