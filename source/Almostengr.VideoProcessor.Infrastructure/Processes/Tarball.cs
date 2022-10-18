using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class Tarball : BaseProcess, ITarball
{
    private const string TarBinary = "/usr/bin/tar";

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
        string tarBallFilePath, string directory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            TarBinary, $"-xvf \"{tarBallFilePath}\" -C \"{directory}\"", directory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new TarballExtractingException("Errors occurred when running the command");
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}
