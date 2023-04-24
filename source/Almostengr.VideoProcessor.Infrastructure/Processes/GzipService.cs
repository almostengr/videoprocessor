using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class GzipService : BaseProcess<GzipService>, IFileCompressionService, IGzFileCompressionService
{
    private const string GZIP = "/usr/bin/gzip";
    private const string GUNZIP = "/usr/bin/gunzip";

    public GzipService(ILoggerService<GzipService> loggerService) : base(loggerService)
    {
    }

    public async Task<(string stdOut, string stdErr)> CompressFileAsync(
        string filePath, CancellationToken cancellationToken)
    {
        if (filePath.EndsWithIgnoringCase(FileExtension.TarXz.Value) || filePath.EndsWithIgnoringCase(FileExtension.TarGz.Value))
        {
            return await Task.FromResult((string.Empty, string.Empty));
        }

        string workingDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        var result = await RunProcessAsync(
            GZIP, $"\"{filePath}\"", workingDirectory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new UnableToCompressFileException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> DecompressFileAsync(
        string filePath, CancellationToken stoppingToken)
    {
        if (!filePath.EndsWithIgnoringCase(FileExtension.TarGz.Value))
        {
            throw new UnableToDecompressFileException();
        }

        string workingDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        var result = await RunProcessAsync(
            GUNZIP, $"\"{filePath}\"", workingDirectory, stoppingToken);

        if (result.exitCode > 0)
        {
            throw new UnableToDecompressFileException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}