namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface ITarballService
{
    Task<(string stdOut, string stdErr)> AddFileToTarballAsync(
        string tarballFilePath, string filePathToAdd, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken);
    Task<bool> DoesTarballContainFileAsync(string tarballFilePath, string fileNameToLocate, CancellationToken cancellationToken);

    // Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
    //     string tarballFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(
        string tarballFilePath, string workingDirectory, CancellationToken cancellationToken);
    
}