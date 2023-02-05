using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions
{
    [Serializable]
    internal class DetailsTxtFileExistsException : VideoProcessorException
    {
        public DetailsTxtFileExistsException()
        {
        }

        public DetailsTxtFileExistsException(string? message) : base(message)
        {
        }

        public DetailsTxtFileExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected DetailsTxtFileExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}