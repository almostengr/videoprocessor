using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class Tarball : BaseProcess, ITarball
{
    private const string TarBinary = "/bin/tar";

    public async Task<(string stdOut, string stdErr)> CreateTarballFromDirectoryAsync(
        string workingDirectory, CancellationToken cancellationToken)
    {
        var result = await RunProcessAsync(
            TarBinary, 
            $"-c * \"{workingDirectory.Replace(Constants.Whitespace, string.Empty)}{FileExtension.Tar}\"",
            workingDirectory,
            cancellationToken
        );

        if (result.exitCode > 0)
        {
            throw new TarballCreationException();
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(
        string tarBallFilePath, string directory, CancellationToken cancellationToken)
    {
        // using Process process = new Process
        // {
        //     StartInfo = new ProcessStartInfo
        //     {
        //         FileName = TarBinary,
        //         Arguments = $"-xvf \"{tarBallFilePath}\" -C \"{directory}\"",
        //         WorkingDirectory = directory,
        //         UseShellExecute = false,
        //         RedirectStandardOutput = true,
        //         RedirectStandardError = true,
        //         CreateNoWindow = true
        //     }
        // };

        // process.Start();

        // string output = process.StandardOutput.ReadToEnd();
        // string error = process.StandardError.ReadToEnd();

        // await process.WaitForExitAsync(cancellationToken);

        var result = await RunProcessAsync(
            TarBinary, $"-xvf \"{tarBallFilePath}\" -C \"{directory}\"", directory, cancellationToken);

        if (result.exitCode > 0)
        {
            throw new TarballExtractingException("Errors occurred when running the command");
        }

        return await Task.FromResult((result.stdOut, result.stdErr));
    }
}
