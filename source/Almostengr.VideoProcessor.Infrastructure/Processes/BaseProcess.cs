using System.Diagnostics;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public abstract class BaseProcess
{
    internal const string BashBinary = "/bin/bash";

    internal async Task<(int exitCode, string? stdOut, string? stdErr)> RunProcessAsync(
        string binary, string arguments, string workingDirectory, CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(binary))
        {
            throw new ProgramPathIsInvalidException();
        }

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ProgramWorkingDirectoryIsInvalidException();
        }

        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = binary,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        await process.WaitForExitAsync(cancellationToken);

        return await Task.FromResult((process.ExitCode, output, error));
    }
}