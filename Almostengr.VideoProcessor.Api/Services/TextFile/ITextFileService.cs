namespace Almostengr.VideoProcessor.Api.Services.TextFile
{
    public interface ITextFileService
    {
        string GetFileContents(string filePath);
        void SaveFileContents(string filePath, string content);
    }
}