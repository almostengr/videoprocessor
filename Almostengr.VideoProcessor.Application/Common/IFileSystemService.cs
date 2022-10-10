namespace Almostengr.VideoProcessor.Application.Common;

public interface IFileSystemService
{
    bool IsDiskSpaceAvailable(string directory);
    string? GetRandomTarballFromDirectory(string directory);
    Task ExtractTarball(string tarballFilePath, string extractDirectory);
    void DeleteDirectory(string directory);
    void CreateDirectory(string directory);
    string CreateFfmpegInputFile(string workingDirectory);
    void ExtractTarballContents(string tarBall, string directory);
    void RenderVideo(string directory, string ffmpegParameters);
    void MoveFile(string sourceFilePath, string destinationDirectory);
}