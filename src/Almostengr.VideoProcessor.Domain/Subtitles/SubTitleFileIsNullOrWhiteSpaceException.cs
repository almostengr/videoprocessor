using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

[Serializable]
internal sealed class SubTitleFileIsNullOrWhiteSpaceException : VideoProcessorException
{
    public SubTitleFileIsNullOrWhiteSpaceException()
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(string message) : base(message)
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(string message, Exception? innerException) : base(message, innerException)
    {
    }

    public SubTitleFileIsNullOrWhiteSpaceException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}