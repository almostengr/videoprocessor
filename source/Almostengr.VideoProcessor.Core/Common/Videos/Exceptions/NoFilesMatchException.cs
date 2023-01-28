using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

[Serializable]
public class NoFilesMatchException : Exception
{
    public NoFilesMatchException()
    {
    }

    public NoFilesMatchException(string? message) : base(message)
    {
    }

    public NoFilesMatchException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected NoFilesMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}