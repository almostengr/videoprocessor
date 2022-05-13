using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services.Subtitles
{
    public interface IBaseSubtitleService : IBaseService
    {
        bool IsValidTranscript(SubtitleInputDto inputDto);
        string CleanBlogString(string blogText);
        string ProcessSentenceCase(string input);
        string RemoveDupesFromBlogString(string blogText);
    }
}