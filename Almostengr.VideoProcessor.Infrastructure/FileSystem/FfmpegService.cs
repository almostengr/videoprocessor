using System.Diagnostics;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class FfmpegService : IFfmpegService
{
    public const string FfmpegBinary = "/usr/bin/ffmpeg";
    public const string FfprobeBinary = "/usr/bin/ffprobe";
    private readonly IFileSystemService _fileSystemService;

    public FfmpegService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public void CreateFfmpegInputFile(string workingDirectory)
    {
        var files = _fileSystemService.GetFilesInDirectory(workingDirectory);
    }

    public async Task<(string stdOut, string stdErr)> FfprobeAsync(string videoFileName, string workingDirectory, CancellationToken cancellationToken)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FfprobeBinary,
                Arguments = $"-hide_banner \"{videoFileName}\"",
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

        await process.WaitForExitAsync();

        if (process.ExitCode > 0)
        {
            throw new FfprobeException("Errors occurred when running the command");
        }

        return await Task.FromResult((output, error));
    }

    public async Task<(string stdOut, string stdErr)> FfmpegAsync(string arguments, string directory, CancellationToken cancellationToken)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegBinary,
                Arguments = $"-hide_banner \"{arguments}\"",
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
            throw new FfmpegRenderVideoException("Errors occurred when running the command");
        }

        return await Task.FromResult((output, error));
    }
}
