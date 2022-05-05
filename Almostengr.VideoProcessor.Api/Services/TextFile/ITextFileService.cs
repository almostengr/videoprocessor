namespace Almostengr.VideoProcessor.Services
{
    public interface ITextFileService 
    {
        string GetFileContents(string filePath);
        void SaveFileContents(string filePath, string content);
        void DeleteFile(string filePath);
    }
}