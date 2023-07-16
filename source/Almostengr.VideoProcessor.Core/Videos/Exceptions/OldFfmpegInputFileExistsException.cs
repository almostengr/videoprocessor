using System.Runtime.Serialization;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos.Exceptions
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