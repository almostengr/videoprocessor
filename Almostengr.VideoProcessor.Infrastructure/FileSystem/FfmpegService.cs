using System.Diagnostics;
using Almostengr.VideoProcessor.Application.Common;
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

    public async Task<(string stdOut, string stdErr)> RenderVideoAsync(string arguments, string workingDirectory)
    {
        using Process process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegBinary,
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

        await process.WaitForExitAsync();

        // _logger.LogInformation($"Exit code: {process.ExitCode}");

        // int errorCount = error.Split("\n")
        //     .Where(x =>
        //         !x.Contains("libva: /usr/lib/x86_64-linux-gnu/dri/iHD_drv_video.so init failed") &&
        //         !x.Contains("Output file is empty, nothing was encoded (check -ss / -t / -frames parameters if used") &&
        //         !x.Contains("deprecated pixel format used, make sure you did set range correctly") &&
        //         !x.Contains("No support for codec hevc profile 1") &&
        //         !x.Contains("Failed setup for format vaapi_vld: hwaccel initialisation returned error") &&
        //         !x.Equals("")
        //     )
        //     .ToArray()
        //     .Count();

        if (process.ExitCode > 0)
        {
            // _logger.LogError(error);
            // throw new ArgumentException("Errors occurred when running the command");
            throw new FfmpegRenderVideoException("Errors occurred when running the command");
        }

        return await Task.FromResult((output, error));
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
                // Arguments = videoFileName,
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
                FileName = FfprobeBinary,
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
            throw new FfprobeException("Errors occurred when running the command");
        }

        return await Task.FromResult((output, error));
    }
}
