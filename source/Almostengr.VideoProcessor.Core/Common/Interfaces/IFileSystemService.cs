using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFileSystemService
{
    void DeleteDirectory(string directory);
    void CreateDirectory(string directory);
    void MoveFile(string sourceFilePath, string destinationDirectory);
    IEnumerable<string> GetFilesInDirectory(string directory);
    IEnumerable<string> GetDirectoriesInDirectory(string directory);
    void PrepareAllFilesInDirectory(string directory);
    void DeleteFile(string ffmpegInputFilePath);
    void SaveFileContents(string filePath, string content);
    FileInfo[] GetFilesInDirectoryWithFileInfo(string directory);
    IEnumerable<string> GetTarballFilesInDirectory(string directory);
    IEnumerable<FileInfo> GetVideoFilesInDirectoryWithFileInfo(string directory);
}