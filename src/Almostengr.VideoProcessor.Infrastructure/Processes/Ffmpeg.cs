using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class Ffmpeg : BaseProcessService, IFfmpeg
{
    private const string LOG_ERRORS = "-loglevel error";
    private const string LOG_WARNINGS = "-loglevel warning";
    private const string FfmpegBinary = "/usr/bin/ffmpeg";
    private const string FfprobeBinary = "/usr/bin/ffprobe";
    private readonly IFileSystem _fileSystem;

    public Ffmpeg(IFileSystem fileSystemService) : base()
    {
        _fileSystem = fileSystemService;
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

    public async Task<(string stdOut, string stdErr)> FfmpegAsync(
        string arguments, string directory, CancellationToken cancellationToken)
    {
        arguments = $"-hide_banner \"{arguments}\"";
        var results = await RunProcessAsync(FfmpegBinary, arguments, directory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfmpegRenderVideoException("Errors occurred when running the command");
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFile, string videoFilter, string outputFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        string arguments =
            $"-y -hide_banner {LOG_ERRORS} -safe 0 -init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i {ffmpegInputFile} -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ImagesToVideoAsync(
        string imagePath, string outputFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        const int duration = 3;

        return await FfmpegAsync(
            $"{LOG_ERRORS} -framerate 1/{duration} -i \"{imagePath}\" -c:v libx264 -t {duration} \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    // public async Task<(string stdout, string stdErr)> AddAudioToVideoAsync(
    // string videoFilePath, string audioFilePath, string outputFilePath, string workingDirectory, CancellationToken cancellationToken)
    // {
    //     return await FfmpegAsync(
    //         $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
    //         workingDirectory,
    //         cancellationToken
    //     );
    // }

    public async Task<(string stdout, string stdErr)> AddAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        return await FfmpegAsync(
            $"{LOG_ERRORS} -hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }


}
