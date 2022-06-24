using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Core.Services.Subtitles
{
    public interface ISubtitleService
    {
        bool IsValidFile(SubtitleInputDto inputDto);
        string CleanBlogString(string blogText);
        string ConvertToSentenceCase(string input);
        string RemoveDuplicatesFromBlogString(string blogText);
        string GetFileContents(string filePath);
        void SaveFileContents(string filePath, string content);
    }
}