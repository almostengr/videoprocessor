using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFileSystemService
{
    bool IsDiskSpaceAvailable(string directory);
    void DeleteDirectory(string directory);
    void CreateDirectory(string directory);
    void MoveFile(string sourceFilePath, string destinationDirectory);
    IEnumerable<string> GetFilesInDirectory(string directory);
    IEnumerable<string> GetDirectoriesInDirectory(string directory);
    void PrepareAllFilesInDirectory(string directory);
    void DeleteFile(string ffmpegInputFilePath);
    void SaveFileContents(string filePath, string content);
    string? GetRandomFileByExtensionFromDirectory(string directory, FileExtension extension);
    FileInfo[] GetFilesInDirectoryWithFileInfo(string directory);
}