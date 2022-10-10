namespace Almostengr.VideoProcessor.Application.Common;

public interface ITarballService
{
    Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(string tarballFilePath, string workingDirectory, CancellationToken cancellationToken);
}