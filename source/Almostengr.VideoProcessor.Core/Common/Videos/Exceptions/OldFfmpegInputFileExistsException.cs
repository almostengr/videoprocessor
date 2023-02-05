using System.Runtime.Serialization;

namespace Almostengr.VideoProcessor.Core.Common.Videos.Exceptions
{
    [Serializable]
    internal class OldFfmpegInputFileExistsException : VideoProcessorException
    {
        public OldFfmpegInputFileExistsException()
        {
        }

        public OldFfmpegInputFileExistsException(string? message) : base(message)
        {
        }

        public OldFfmpegInputFileExistsException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected OldFfmpegInputFileExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}