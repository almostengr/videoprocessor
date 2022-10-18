namespace Almostengr.VideoProcessor.Domain.Interfaces;

public interface ITarball
{
    Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string tarballFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(
        string tarballFilePath, string workingDirectory, CancellationToken cancellationToken);
}