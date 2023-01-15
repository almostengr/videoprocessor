using Almostengr.VideoProcessor.Core.Common;
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
        string ffmpegInputFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> RenderVideoWithMixTrackAsync(
        string ffmpegInputFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -i \"{audioTrackFilePath}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"";

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
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
    }

    private bool IsVideoFile(string fileName)
    {
        return (fileName.EndsWith(FileExtension.Avi) || fileName.EndsWith(FileExtension.Mkv) ||
            fileName.EndsWith(FileExtension.Mov) || fileName.EndsWith(FileExtension.Mp4));
    }

    public async Task<(string stdout, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, string videoFilter, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            // $"-f concat -i \"{ffmpegInputFilePath}\" -c copy -bsf:a aac_adtstoasc {outputFilePath}",
            $"-f concat -safe 0 -i \"{ffmpegInputFilePath}\" -c:a copy -bsf:a aac_adtstoasc -vf \"{videoFilter}\" \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken);
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
        string outputFileName =
            Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.Ts);

        return await FfmpegAsync(
            $"-i \"{videoFilePath}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{outputFileName}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken)
    {
        return await FfmpegAsync(
            $"-i \"{videoInputFilePath}\" -vn \"{audioOutputFilePath}\"",
            // $"-i \"{videoInputFilePath}\" -vn -acodec copy \"{audioOutputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ConvertEndScreenImageToMp4VideoAsync(
        string endScreenImageFilePath, string endScreenAudioFilePath, string endScreenOutputFilePath, CancellationToken cancellationToken)
    {
        if (!endScreenAudioFilePath.EndsWith(FileExtension.Mp3))
        {
            throw new VideoProcessorException("Audio file in wrong format");
        }

        if (!endScreenImageFilePath.EndsWith(FileExtension.Png) && !endScreenImageFilePath.EndsWith(FileExtension.Jpg))
        {
            throw new VideoProcessorException("Image file in wrong format");
        }

        if (!endScreenOutputFilePath.EndsWith(FileExtension.Mp4))
        {
            throw new VideoProcessorException("Output file must be in MP4 format");
        }

        // ffmpeg -y -framerate 0.1 -i endscreen_techtalk.png -i endscreen.mp3 -map 0:v:0 -map 1:a:0 -c:v libx264 -r 30 -shortest endtechtalk.mp4
        string workingDirectory = Path.GetDirectoryName(endScreenImageFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();

        return await FfmpegAsync(
            // $"-framerate 0.1 -i \"{endScreenImageFilePath}\" -i \"{endScreenAudioFilePath}\" -map 0:v:0 -map 1:a:0 -c:v h264_mp4toannexb -r 30 -shortest \"{endScreenOutputFilePath}\"",
            $"-y -framerate 0.1 -i \"{endScreenImageFilePath}\" -i \"{endScreenAudioFilePath}\" -map 0:v:0 -map 1:a:0 -c:v libx264 -r 30 -shortest \"{endScreenOutputFilePath}\"",
            workingDirectory,
            cancellationToken);
    }
}
