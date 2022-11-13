using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class Tarball : BaseProcess<Tarball>, ITarball
{
    private const string TarBinary = "/usr/bin/tar";

    public Tarball(ILoggerService<Tarball> loggerService) : base(loggerService)
    {
    }

    public async Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string tarballFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            BashBinary,
            $"-c \"cd \\\"{workingDirectory}\\\" && {TarBinary} -cvJf \\\"{tarballFilePath}\\\" *\"",
            workingDirectory,
            cancellationToken
        );

        if (result.exitCode > 0)
        {
            throw new TarballCreationException(result.stdErr);
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            BashBinary,
            $"-c \"cd \\\"{workingDirectory}\\\" && {TarBinary} -cvJf \\\"{workingDirectory + FileExtension.TarXz}\\\" *\"",
            workingDirectory,
            cancellationToken
        );

        if (result.exitCode > 0)
        {
            throw new TarballCreationException(result.stdErr);
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(
        string tarballFilePath, string directory, CancellationToken cancellationToken)
    {
        FileInfo fileInfo = new FileInfo(tarballFilePath);
        if (fileInfo.Exists == false)
        {
            throw new TarballExtractingException($"{nameof(tarballFilePath)} does not exist");
        }

        var result = await RunProcessAsync(
            TarBinary, $"-xvf \"{tarballFilePath}\" -C \"{directory}\"", directory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new TarballExtractingException(result.stdErr);
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}
