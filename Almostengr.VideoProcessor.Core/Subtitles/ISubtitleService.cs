using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Subtitles
{
    public interface ISubtitleService 
    {
        string CleanBlogString(string blogText);
        string ConvertToSentenceCase(string input);
        string RemoveDuplicatesFromBlogString(string blogText);
        string GetFileContents(string filePath);
        void SaveFileContents(string filePath, string content);
    }
}