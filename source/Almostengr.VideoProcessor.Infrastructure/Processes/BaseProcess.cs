using System.Diagnostics;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public abstract class BaseProcess<T>
{
    private readonly ILoggerService<T> _loggerService;
    internal const string BASH_BINARY = "/bin/bash";
    internal const string GREP_BINARY = "/usr/bin/grep";

    protected BaseProcess(ILoggerService<T> loggerService)
    {
        _loggerService = loggerService;
    }

    internal async Task<(int exitCode, string stdOut, string stdErr)> RunProcessAsync(
        string binary, string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(binary))
        {
            throw new ArgumentException("Program path is not valid", nameof(binary));
        }

        if (string.IsNullOrWhiteSpace(workingDirectory))
        {
            throw new ProgramWorkingDirectoryIsInvalidException();
        }

        _loggerService.LogInformation($"Running {binary} {arguments}");

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

        _loggerService.LogInformation($"Done running {binary} {arguments}");

        return await Task.FromResult((process.ExitCode, output, error));
    }
}