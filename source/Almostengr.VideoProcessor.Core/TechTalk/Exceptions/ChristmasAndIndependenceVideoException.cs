using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.TechTalk.Exceptions;

[Serializable]
internal class ChristmasAndIndependenceVideoException : VideoProcessorException
{
    public ChristmasAndIndependenceVideoException()
    {
    }

    public ChristmasAndIndependenceVideoException(string? message) : base(message)
    {
    }

    public ChristmasAndIndependenceVideoException(string? message, Exception? innerException) : base(message, innerException)
    {
    }

    protected ChristmasAndIndependenceVideoException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}