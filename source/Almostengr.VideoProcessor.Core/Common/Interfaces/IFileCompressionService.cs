namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFileCompressionService
{
    Task<(string stdOut, string stdErr)> CompressFileAsync(string tarballFile, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> DecompressFileAsync(string tarballFilePath, CancellationToken stoppingToken);
}