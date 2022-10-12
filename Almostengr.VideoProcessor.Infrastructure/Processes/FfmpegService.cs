using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class FfmpegService : BaseProcessService, IFfmpegService
{
    private const string FfmpegBinary = "/usr/bin/ffmpeg";
    private const string FfprobeBinary = "/usr/bin/ffprobe";
    private readonly IFileSystemService _fileSystemService;

    public FfmpegService(IFileSystemService fileSystemService) : base()
    {
        _fileSystemService = fileSystemService;
    }

    public async Task<(string stdOut, string stdErr)> FfprobeAsync(string videoFileName, string workingDirectory, CancellationToken cancellationToken)
    {
        string arguments = $"-hide_banner \"{videoFileName}\"";
        var results = await RunProcessAsync(FfprobeBinary, arguments, workingDirectory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfprobeException("Errors occurred when running the command");
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> FfmpegAsync(string arguments, string directory, CancellationToken cancellationToken)
    {
        arguments = $"-hide_banner \"{arguments}\"";
        var results = await RunProcessAsync(FfmpegBinary, arguments, directory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfmpegRenderVideoException("Errors occurred when running the command");
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }
}
