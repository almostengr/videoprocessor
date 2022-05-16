namespace Almostengr.VideoProcessor.Api.Services.TextFile
{
    public interface ITextFileService : IBaseService
    {
        string GetFileContents(string filePath);
        void SaveFileContents(string filePath, string content);
    }
}