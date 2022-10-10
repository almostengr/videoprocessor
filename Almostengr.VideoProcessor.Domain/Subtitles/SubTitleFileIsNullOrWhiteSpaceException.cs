using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal class SubTitleFileIsNullOrWhiteSpaceException : Exception
{
    public SubTitleFileIsNullOrWhiteSpaceException()
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(string? message) : base(message)
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected SubTitleFileIsNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}