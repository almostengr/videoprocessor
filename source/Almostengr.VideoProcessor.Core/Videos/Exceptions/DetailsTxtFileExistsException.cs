using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions
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