using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
[Serializable]
internal class ProjectFileDoesNotExistException : Exception
{
    public ProjectFileDoesNotExistException()
    {
    }

    public ProjectFileDoesNotExistException(string? message) : base(message)
    {
    }

    public ProjectFileDoesNotExistException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ProjectFileDoesNotExistException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}