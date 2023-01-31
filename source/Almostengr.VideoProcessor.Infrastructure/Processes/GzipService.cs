using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;
using Almostengr.VideoProcessor.Core.Common.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class GzipService : BaseProcess<GzipService>, IFileCompressionService, IGzFileCompressionService
{
    private const string Gzip = "/usr/bin/gzip";
    private const string Gunzip = "/usr/bin/gunzip";

    public GzipService(ILoggerService<GzipService> loggerService) : base(loggerService)
    {
    }

    public async Task<(string stdOut, string stdErr)> CompressFileAsync(
        string filePath, CancellationToken cancellationToken)
    {
        if (filePath.EndsWith(FileExtension.TarXz.ToString()) || filePath.EndsWith(FileExtension.TarGz.ToString()))
        {
            throw new FileAlreadyCompressedException();
        }

        string workingDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        var result = await RunProcessAsync(
            Gzip, $"\"{filePath}\"", workingDirectory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new UnableToCompressFileException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> DecompressFileAsync(
        string filePath, CancellationToken stoppingToken)
    {
        if (!filePath.EndsWith(FileExtension.TarGz.ToString()))
        {
            throw new UnableToDecompressFileException();
        }

        string workingDirectory = Path.GetDirectoryName(filePath) ?? string.Empty;

        var result = await RunProcessAsync(
            Gunzip, $"\"{filePath}\"", workingDirectory, stoppingToken);

        if (result.exitCode > 0)
        {
            throw new UnableToDecompressFileException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}