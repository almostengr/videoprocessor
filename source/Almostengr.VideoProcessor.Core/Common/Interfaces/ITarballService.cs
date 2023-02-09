namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface ITarballService
{
    Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(
        string tarballFilePath, string workingDirectory, CancellationToken cancellationToken);
    
}