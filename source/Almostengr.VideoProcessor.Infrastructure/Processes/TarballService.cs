using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class TarballService : BaseProcess<TarballService>, ITarballService
{
    private const string TAR_BINARY = "/usr/bin/tar";

    public TarballService(ILoggerService<TarballService> loggerService) : base(loggerService)
    {
    }

    public async Task<(string stdOut, string stdErr)> AddFileToTarballAsync(
            string tarballFilePath, string filePathToAdd, string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            TAR_BINARY,
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

    public async Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            BASH_BINARY,
            $"-c \"cd \\\"{workingDirectory}\\\" && {TAR_BINARY} -cvf \\\"{workingDirectory + FileExtension.Tar.Value}\\\" *\"",
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
            TAR_BINARY, $"-xvf \"{tarballFilePath}\" -C \"{directory}\"", directory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new TarballExtractingException(result.stdErr);
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}
