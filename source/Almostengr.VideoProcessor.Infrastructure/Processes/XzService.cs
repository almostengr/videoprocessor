using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Exceptions;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class XzService : BaseProcess<XzService>, IFileCompressionService, IXzFileCompressionService
{
    private const string Xz = "/usr/bin/xz";
    
    public XzService(ILoggerService<XzService> loggerService) : base(loggerService)
    {
    }

    public async Task<(string stdOut, string stdErr)> CompressFileAsync(
        string tarballFilePath, CancellationToken cancellationToken)
    {
        if (tarballFilePath.EndsWith(FileExtension.TarXz.Value) || tarballFilePath.EndsWith(FileExtension.TarGz.Value))
        {
            throw new FileAlreadyCompressedException();
        }

        string workingDirectory = Path.GetDirectoryName(tarballFilePath) ?? string.Empty;

        var result = await RunProcessAsync(
            Xz, $"-z \"{tarballFilePath}\"", workingDirectory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new UnableToCompressFileException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> DecompressFileAsync(
        string tarballFilePath, CancellationToken stoppingToken)
    {
        if (!tarballFilePath.EndsWith(FileExtension.TarXz.Value))
        {
            throw new UnableToDecompressFileException();
        }

        string workingDirectory = Path.GetDirectoryName(tarballFilePath) ?? string.Empty;

        var result = await RunProcessAsync(
            Xz, $"-d \"{tarballFilePath}\"", workingDirectory, stoppingToken);

        if (result.exitCode > 0)
        {
            throw new UnableToDecompressFileException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}