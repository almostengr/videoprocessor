using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class FfmpegService : BaseProcess<FfmpegService>, IFfmpegService
{
    private const string FfmpegBinary = "/usr/bin/ffmpeg";
    private const string FfprobeBinary = "/usr/bin/ffprobe";
    private readonly IFileSystemService _fileSystem;

    public FfmpegService(IFileSystemService fileSystemService, ILoggerService<FfmpegService> loggerService) :
        base(loggerService)
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
            throw new FfprobeException(results.stdErr);
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> FfmpegAsync(
        string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
        arguments = $"-y -hide_banner -loglevel error {arguments}";
        var results = await RunProcessAsync(FfmpegBinary, arguments, workingDirectory, cancellationToken);

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
        if (!string.IsNullOrEmpty(videoFilter))
        {
            videoFilter += ",";
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i \"{videoFilePath}\" -i \"{audioTrackFilePath}\" -filter_hw_device foo -vf \"{videoFilter} format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ImagesToVideoAsync(
        string imageFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        const int DURATION = 3;
        string workingDirectory =
            Path.GetDirectoryName(imageFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-framerate 1/{DURATION} -i \"{imageFilePath}\" -c:v libx264 -t {DURATION} \"{outputFilePath}\"",
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
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{videoFilePath}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> AddAccAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v h264_vaapi -c:a aac -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    private bool IsVideoFile(string fileName)
    {
        return (fileName.EndsWith(FileExtension.Avi.Value) || fileName.EndsWith(FileExtension.Mkv.Value) ||
            fileName.EndsWith(FileExtension.Mov.Value) || fileName.EndsWith(FileExtension.Mp4.Value));
    }

    public async Task<(string stdout, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, string videoFilter, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -c:a copy -bsf:a aac_adtstoasc -vf \"{videoFilter}\" \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -c:v copy -c:a copy -bsf:a aac_adtstoasc \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ConvertMp4VideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(videoFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new InvalidPathException("Invalid paths provided");
        }

        if (!IsVideoFile(videoFilePath))
        {
            return ("Not a video file", string.Empty);
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-i \"{videoFilePath}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }
    public async Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
    {

        if (string.IsNullOrWhiteSpace(videoFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new InvalidPathException("Invalid paths provided");
        }

        if (!IsVideoFile(videoFilePath))
        {
            return ("Not a video file", string.Empty);
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            $"-i \"{videoFilePath}\" -c copy -f mpegts \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        return await FfmpegAsync(
            $"-i \"{videoInputFilePath}\" -vn \"{audioOutputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> RenderVideoWithFiltersAsync(
        string ffmpegInputFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -vf \"{videoFilters}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }
}
