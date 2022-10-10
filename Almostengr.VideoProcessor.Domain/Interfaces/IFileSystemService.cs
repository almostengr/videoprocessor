namespace Almostengr.VideoProcessor.Domain.Interfaces;

public interface IFileSystemService
{
    bool IsDiskSpaceAvailable(string directory);
    string? GetRandomTarballFromDirectory(string directory);
    void DeleteDirectory(string directory);
    void CreateDirectory(string directory);
    void MoveFile(string sourceFilePath, string destinationDirectory);
    IEnumerable<string> GetFilesInDirectory(string directory);
    IEnumerable<string> GetDirectoriesInDirectory(string directory);
    void PrepareAllFilesInDirectory(string directory);
    void DeleteFile(string ffmpegInputFilePath);
    bool DoesFileExist(string filePath);
    void DeleteFiles(string[] narrationFiles);
}