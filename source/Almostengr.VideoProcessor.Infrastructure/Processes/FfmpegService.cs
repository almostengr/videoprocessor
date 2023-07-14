using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using System.Text;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class FfmpegService : BaseProcess<FfmpegService>, IFfmpegService
{
    private const string FFMPEG_BINARY = "/usr/bin/ffmpeg";
    private const string FFPROBE_BINARY = "/usr/bin/ffprobe";
    private const string HARDWARE_RENDERING_FAILED = "Hardware rendering failed. Using CPU rendering.";
    private readonly IFileSystemService _fileSystem;
    private readonly ILoggerService<FfmpegService> _logger;

    public FfmpegService(IFileSystemService fileSystemService, ILoggerService<FfmpegService> loggerService) :
        base(loggerService)
    {
        _fileSystem = fileSystemService;
        _logger = loggerService;
    }

    public async Task<(string stdOut, string stdErr)> FfprobeAsync(
        string videoFileName, string workingDirectory, CancellationToken cancellationToken)
    {
        string arguments = $"-hide_banner \"{videoFileName}\"";
        var results = await RunProcessAsync(FFPROBE_BINARY, arguments, workingDirectory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfprobeException(results.stdErr);
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    private async Task<(string stdOut, string stdErr)> FfmpegAsync(
        string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        _ = arguments ?? throw new ArgumentException(nameof(arguments));
        _ = workingDirectory ?? throw new ArgumentException(nameof(workingDirectory));

        StringBuilder args = new();
        args.Append("-y -hide_banner ");
        if (!arguments.Contains("volumedetect"))
        {
            args.Append("-loglevel error ");
        }
        args.Append(arguments);

        var results = await RunProcessAsync(FFMPEG_BINARY, args.ToString(), workingDirectory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfmpegRenderVideoException(results.stdErr);
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoAsCopyAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -c copy \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -vf \"format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoWithAudioAndFiltersAsync(
        string videoFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        if (videoFilter.IsNotNullOrWhiteSpace())
        {
            videoFilter += ",";
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i \"{videoFilePath}\" -i \"{audioTrackFilePath}\" -filter_hw_device foo -vf \"{videoFilter} format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> AddAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{videoFilePath}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdOut, string stdErr)> AddAccAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v h264_vaapi -c:a aac -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdOut, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, string videoFilter, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -c:a copy -bsf:a aac_adtstoasc -vf \"{videoFilter}\" \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -c:v copy -c:a copy -bsf:a aac_adtstoasc \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> ConvertMp4VideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(videoFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new InvalidPathException("Invalid paths provided");
        }

        if (!videoFilePath.IsVideoFile())
        {
            throw new ArgumentException("Not a video file", nameof(videoFilePath));
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-i \"{videoFilePath}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }


    public async Task<(string stdOut, string stdErr)> ConvertVideoFileToTsFormatWithFiltersAsync(
        string videoFilePath, string outputFilePath, string videoFilters, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(videoFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new ArgumentException("Invalid path(s) provided");
        }

        if (!videoFilePath.IsVideoFile())
        {
            throw new ArgumentException("Not a video file", nameof(videoFilePath));
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments = $"-i \"{videoFilePath}\" -vf \"{videoFilters}\" -f mpegts \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(videoFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new ArgumentException("Invalid path(s) provided", nameof(videoFilePath));
        }

        if (!videoFilePath.IsVideoFile())
        {
            throw new ArgumentException("Not a video file", nameof(videoFilePath));
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments = $"-i \"{videoFilePath}\" -c:v copy -f mpegts \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        return await FfmpegAsync(
            $"-i \"{videoInputFilePath}\" -vn \"{audioOutputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoWithFiltersAsync(
        string videoFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken)
    {
        _ = videoFilePath ?? throw new ArgumentException(nameof(videoFilePath));
        _ = videoFilters ?? throw new ArgumentException(nameof(videoFilters));
        _ = outputFilePath ?? throw new ArgumentException(nameof(outputFilePath));
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i \"{videoFilePath}\" -filter_hw_device foo -vf \"{videoFilters}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        try
        {
            return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
        }
        catch (FfmpegRenderVideoException)
        {
            arguments = $"-i \"{videoFilePath}\" -vf \"{videoFilters}\" \"{outputFilePath}\"";
            return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
        }
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoWithInputFileAndFiltersAsync(
        string ffmpegInputFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken)
    {
        _ = ffmpegInputFilePath ?? throw new ArgumentException(nameof(ffmpegInputFilePath));
        _ = videoFilters ?? throw new ArgumentException(nameof(videoFilters));
        _ = outputFilePath ?? throw new ArgumentException(nameof(outputFilePath));
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -vf \"{videoFilters}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        try
        {
            return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
        }
        catch (FfmpegRenderVideoException)
        {
            _logger.LogWarning(HARDWARE_RENDERING_FAILED);
            arguments = $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -vf \"{videoFilters}\" \"{outputFilePath}\"";
            return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
        }
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoWithInputFileAndAudioAndFiltersAsync(
        string ffmpegInputFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        _ = ffmpegInputFilePath ?? throw new ArgumentException(nameof(ffmpegInputFilePath));
        _ = audioTrackFilePath ?? throw new ArgumentException(nameof(audioTrackFilePath));
        _ = outputFilePath ?? throw new ArgumentException(nameof(outputFilePath));

        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -i \"{audioTrackFilePath}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"";

        try
        {
            return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
        }
        catch (FfmpegRenderVideoException)
        {
            _logger.LogWarning(HARDWARE_RENDERING_FAILED);
            arguments = $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -i \"{audioTrackFilePath}\" -vf \"{videoFilter}\" -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"";
            return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
        }
    }

    public async Task<(string stdOut, string stdErr)> AnalyzeAudioVolumeAsync(string inputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(inputFilePath) ?? throw new ArgumentException("Invalid directory", nameof(inputFilePath));
        string arguments = $"-i \"{inputFilePath}\" -af \"volumedetect\" -vn -sn -dn -f null /dev/null";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> AdjustAudioVolumeAsync(string inputFilePath, string outputFilePath, float maxVolume, CancellationToken cancellationToken)
    {
        if (!outputFilePath.EndsWithIgnoringCase(FileExtension.Mp3.Value) || string.IsNullOrEmpty(outputFilePath))
        {
            throw new ArgumentException("Invalid output file name", nameof(outputFilePath));
        }

        string workingDirectory = Path.GetDirectoryName(inputFilePath) ?? throw new ArgumentException("Invalid working directory", nameof(inputFilePath));
        const float NORMALIZED_DB_THRESHOLD = -3.0F;
        int adjustmentDb = (int)(NORMALIZED_DB_THRESHOLD - maxVolume);

        string arguments = $"-i \"{inputFilePath}\" -af \"volume={adjustmentDb}dB\" \"{outputFilePath}\" ";
        if (adjustmentDb < 1)
        {
            arguments = $"-i \"{inputFilePath}\" \"{outputFilePath}\" ";
        }

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> RenderTsVideoFileFromImageAsync(string imageFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        _ = outputFilePath ?? throw new ArgumentException("invalid output path", nameof(outputFilePath));
        string workingDirectory = Path.GetDirectoryName(imageFilePath) ?? throw new ArgumentException("Invalid working directory", nameof(imageFilePath));
        const int CLIP_DURATION = 4;
        string arguments = $"-loop 1 -i \"{imageFilePath}\" -c:v libx264 -t {CLIP_DURATION} -s 1920x1080 -pix_fmt yuv420p -f mpegts \"{outputFilePath}\"";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> RenderTimelapseVideoAsync(string videoFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(videoFilePath) ?? throw new ArgumentException("Invalid working directory", nameof(videoFilePath));
        string outputFilePath =
            Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(videoFilePath) + ".tmp" + Path.GetExtension(videoFilePath));
        string arguments = $"-i \"{videoFilePath}\" -filter:v \"setpts=0.5*PTS\" -an \"{outputFilePath}\"";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> ConvertImageFileToVideoAsync(string imageFile, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(outputFilePath) ?? throw new ArgumentException("Invalid working directory", nameof(outputFilePath));
        const int DURATION = 3;
        string arguments = $"-framerate 1/{DURATION} -pattern_type glob -i \"{imageFile}\" libx264 -r 30 -pix_fmt yuv420p {outputFilePath}";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> CreateMusicMixTrackAsync(
        string ffmpegInputFile, string outputFile, string workingDirectory, CancellationToken cancellationToken)
    {
        _ = ffmpegInputFile ?? throw new ArgumentNullException(nameof(ffmpegInputFile));
        _ = outputFile ?? throw new ArgumentNullException(nameof(outputFile));
        _ = workingDirectory ?? throw new ArgumentException(nameof(workingDirectory));

        string arguments = $"-i \"{ffmpegInputFile}\" -c:a copy \"{outputFile}\"";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }
}
