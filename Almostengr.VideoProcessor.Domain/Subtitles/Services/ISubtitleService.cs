namespace Almostengr.VideoProcessor.Domain.Subtitles;
public interface ISubtitleService
{
    // string CleanBlogString(string blogText);
    // string ConvertToSentenceCase(string input);
    // string RemoveDuplicatesFromBlogString(string blogText);
    // bool IsDiskSpaceAvailable(string incomingDirectory, double threshold);
    // abstract void SaveFileContents(string filePath, string content);
    // abstract void GetFileContents(string filePath);
    abstract Task ExecuteAsync(CancellationToken cancellationToken);
}