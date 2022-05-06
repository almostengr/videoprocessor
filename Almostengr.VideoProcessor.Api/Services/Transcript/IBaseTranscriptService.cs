using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IBaseTranscriptService
    {
        bool IsValidTranscript(TranscriptInputDto inputDto);
        string CleanBlogString(string blogText);
        string ProcessSentenceCase(string input);
        string RemoveDupesFromBlogString(string blogText);
    }
}