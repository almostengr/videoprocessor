using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class Ffmpeg : BaseProcess, IFfmpeg
{
    private const string LOG_ERRORS = "-loglevel error";
    private const string FfmpegBinary = "/usr/bin/ffmpeg";
    private const string FfprobeBinary = "/usr/bin/ffprobe";
    private readonly IFileSystem _fileSystem;

    public Ffmpeg(IFileSystem fileSystemService) : base()
    {
        _fileSystem = fileSystemService;
    }

    public async Task<(string stdOut, string stdErr)> FfprobeAsync(
        string videoFileName, string workingDirectory, CancellationToken cancellationToken)
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
        string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        arguments = $"-y -hide_banner {LOG_ERRORS} -safe 0 \"{arguments}\"";
        var results = await RunProcessAsync(FfmpegBinary, arguments, workingDirectory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfmpegRenderVideoException("Errors occurred when running the command");
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -i {ffmpegInputFilePath} -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ImagesToVideoAsync(
        string imageFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        const int duration = 3;
        string workingDirectory =
            Path.GetDirectoryName(imageFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-framerate 1/{duration} -i \"{imageFilePath}\" -c:v libx264 -t {duration} \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> AddAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string outputFileName = Path.GetFileNameWithoutExtension(outputFilePath) + FileExtension.Ts;

        return await FfmpegAsync(
            $"-i \"{Path.GetFileName(videoFilePath)}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{outputFileName}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> CreateThumbnailsFromVideoAsync(
        string videoTitle, string outputVideoPath, string workingDirectory, CancellationToken cancellationToken)
    {
        const int sceneChangePct = 10;
        const int extractNumberOfFrames = 30;

        return await FfmpegAsync(
            $"-i \"{outputVideoPath}\" -vf select=gt(scene\\,0.{sceneChangePct}) -frames:v {extractNumberOfFrames} -vsync vfr \"{videoTitle}-%03d.jpg\"",
            workingDirectory,
            cancellationToken
        );
    }
}
