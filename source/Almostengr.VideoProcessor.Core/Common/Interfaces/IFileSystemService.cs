using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFileSystemService
{
    bool IsDiskSpaceAvailable(string directory);
    // string GetRandomTarballFromDirectory(string directory);
    void DeleteDirectory(string directory);
    void CreateDirectory(string directory);
    void MoveFile(string sourceFilePath, string destinationDirectory, bool createDestinationDirectory = true);
    IEnumerable<string> GetFilesInDirectory(string directory);
    IEnumerable<string> GetDirectoriesInDirectory(string directory);
    void PrepareAllFilesInDirectory(string directory);
    void DeleteFile(string ffmpegInputFilePath);
    // bool DoesFileExist(string filePath);
    void SaveFileContents(string filePath, string content);
    // string GetFileContents(string filePath);
    // string GetRandomSrtFileFromDirectory(string directory);
    // void CopyFile(string sourceFilePath, string destinationFilePath);
    string GetRandomFileByExtensionFromDirectory(string directory, FileExtension extension);
}