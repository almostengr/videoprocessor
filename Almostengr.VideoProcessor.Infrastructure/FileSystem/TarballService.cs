using System.Diagnostics;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class TarballService : ITarballService
{
    public const string TarBinary = "/bin/tar";

    public async Task<(string stdOut, string stdErr)> ExtractTarballContentsAsync(string tarBallFilePath, string directory, CancellationToken cancellationToken)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = TarBinary,
                // Arguments = videoFileName,
                Arguments = $"-xvf \"{tarBallFilePath}\" -C \"{directory}\"",
                WorkingDirectory = directory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        await process.WaitForExitAsync();

        if (process.ExitCode > 0)
        {
            throw new TarballExtractingException("Errors occurred when running the command");
        }

        return await Task.FromResult((output, error));
    }
}
