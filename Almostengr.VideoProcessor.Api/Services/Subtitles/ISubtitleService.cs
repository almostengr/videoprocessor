using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services.Subtitles
{
    public interface ISubtitleService : IBaseService
    {
        bool IsValidFile(SubtitleInputDto inputDto);
        string CleanBlogString(string blogText);
        string ConvertToSentenceCase(string input);
        string RemoveDuplicatesFromBlogString(string blogText);
    }
}