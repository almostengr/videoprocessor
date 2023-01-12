using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class TarballService : BaseProcess<TarballService>, ITarballService
{
    private const string TarBinary = "/usr/bin/tar";

    public TarballService(ILoggerService<TarballService> loggerService) : base(loggerService)
    {
    }

    public async Task<(string stdOut, string stdErr)> AddFileToTarballAsync(
            string tarballFilePath, string filePathToAdd, string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            TarBinary,
            $"-rf \"{tarballFilePath}\" \"{Path.GetFileName(filePathToAdd)}\"",
            workingDirectory,
            cancellationToken
        );

        if (result.exitCode > 0)
        {
            throw new AddFileToTarballException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<bool> DoesTarballContainFileAsync(
        string tarballFilePath, string fileNameToLocate, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(tarballFilePath) ?? string.Empty;

        var result = await RunProcessAsync(
            BashBinary,
            $"-c \"{TarBinary} -tf \\\"{tarballFilePath}\\\" | {GrepBinary} -i {fileNameToLocate}\"",
            workingDirectory,
            cancellationToken
        );

        return await Task.FromResult(result.stdOut.ToLower().Contains(fileNameToLocate.ToLower()));
    }

    public async Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            BashBinary,
            $"-c \"cd \\\"{workingDirectory}\\\" && {TarBinary} -cvf \\\"{workingDirectory + FileExtension.Tar}\\\" *\"",
            // $"-c \"cd \\\"{workingDirectory}\\\" && {TarBinary} -cvJf \\\"{workingDirectory + FileExtension.TarXz}\\\" *\"",
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
