namespace Almostengr.VideoProcessor.Domain.Subtitles;
public interface ISubtitleService
{
    string CleanBlogString(string blogText);
    string ConvertToSentenceCase(string input);
    string RemoveDuplicatesFromBlogString(string blogText);
    string GetFileContents(string filePath);
    void SaveFileContents(string filePath, string content);
    bool IsDiskSpaceAvailable(string incomingDirectory, double threshold);
}