using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions
{
    [Serializable]
    internal class TarballFilePathException : VideoProcessorException
    {
        public TarballFilePathException()
        {
        }

        public TarballFilePathException(string? message) : base(message)
        {
        }

        public TarballFilePathException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected TarballFilePathException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}