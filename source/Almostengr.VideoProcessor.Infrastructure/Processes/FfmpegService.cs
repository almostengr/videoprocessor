using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using System.Text;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;
using System.IO;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class FfmpegService : BaseProcess<FfmpegService>, IFfmpegService
{
    private const string FFMPEG_BINARY = "/usr/bin/ffmpeg";
    private const string FFPROBE_BINARY = "/usr/bin/ffprobe";
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
        var results = await RunProcessAsync(FFPROBE_BINARY, arguments, workingDirectory, cancellationToken);

        if (results.exitCode > 0)
        {
            throw new FfprobeException(results.stdErr);
        }

        return await Task.FromResult((results.stdOut, results.stdErr));
    }

    public async Task<(string stdOut, string stdErr)> FfmpegAsync(
        string arguments, string workingDirectory, CancellationToken cancellationToken)
    {
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

        // todo - removed "-shortest" to prevent video from being cut off
        return await FfmpegAsync(
            $"-hwaccel vaapi -hwaccel_output_format vaapi -i \"{videoFilePath}\" -i \"{audioFilePath}\" -c:v h264_vaapi -c:a aac -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"",
            workingDirectory,
            cancellationToken
        );
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


    public async Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatWithFiltersAsync(
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
        // string arguments = $"-i \"{videoFilePath}\" -c:v copy -f mpegts \"{outputFilePath}\"";
        string arguments = $"-i \"{videoFilePath}\" -vf \"{videoFilters}\" -f mpegts \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
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
        string arguments = $"-i \"{videoFilePath}\" -c:v copy -f mpegts \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stout, string stdErr)> ConvertImageFileToTsFormatAsync(
        string imageFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(imageFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments = 
            // $"-hwaccel vaapi -vaapi_device /dev/dri/renderD128 -i \"{imageFilePath}\" -vf 'format=nv12,hwupload' -c:v h264_vaapi -b:v 10M -vf \"scale=1920:1080\" -f mpegts \"{outputFilePath}\"";
            // $"-hwaccel vaapi -hwaccel_output_format nv12 -vaapi_device /dev/dri/renderD128 -loop 1 -i \"{imageFilePath}\" -vf 'format=nv12,hwupload' -c:v h264_vaapi -b:v 10M -pix_fmt yuv420p -t 4 -f mpegts \"{outputFilePath}\"";
            // $"-hwaccel vaapi -hwaccel_output_format nv12 -vaapi_device /dev/dri/renderD128 -loop 1 -i \"{imageFilePath}\" -vf 'format=nv12,hwupload' -c:v h264_vaapi -b:v 10M -vf \"scale=1920:1080\" -pix_fmt yuv 420p -t 4 -f mpegts \"{outputFilePath}\"";
            $"-y -hide_banner -loglevel error -hwaccel vaapi -hwaccel_output_format nv12 -vaapi_device /dev/dri/renderD128 -loop 1 -i \"{imageFilePath}\" -vf 'format=nv12,hwupload' -c:v h264_vaapi -b:v 10M -pix_fmt yuv420p -t 4 -f mpegts \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
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
        string videoFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -i \"{videoFilePath}\" -filter_hw_device foo -vf \"{videoFilters}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> RenderVideoWithInputFileAndFiltersAsync(
        string ffmpegInputFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -vf \"{videoFilters}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> RenderVideoWithInputFileAndAudioAndFiltersAsync(
        string ffmpegInputFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -i \"{audioTrackFilePath}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFilePath}\"";

        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> AnalyzeAudioVolumeAsync(string inputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(inputFilePath) ?? throw new ArgumentException("Invalid directory", nameof(inputFilePath));
        string arguments = $"-i \"{inputFilePath}\" -af \"volumedetect\" -vn -sn -dn -f null /dev/null";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdOut, string stdErr)> AdjustAudioVolumeAsync(string inputFilePath, string outputFilePath, float maxVolume, CancellationToken cancellationToken)
    {
        if (!outputFilePath.EndsWithIgnoringCase(FileExtension.Mp3.Value))
        {
            throw new ArgumentException("Invalid output file name");
        }

        const float NORMALIZED_DB_THRESHOLD = -3.0F;
        string workingDirectory = Path.GetDirectoryName(inputFilePath) ?? throw new ArgumentException("Invalid directory", nameof(inputFilePath));
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
        string workingDirectory = Path.GetDirectoryName(imageFilePath) ?? throw new ArgumentException("Invalid directory", nameof(imageFilePath));
        _ = outputFilePath ?? throw new ArgumentException("invalid output path", nameof(outputFilePath));
        const int CLIP_DURATION = 4;
        string arguments = $"-loop 1 -i \"{imageFilePath}\" -c:v libx264 -t {CLIP_DURATION} -s 1920x1080 -pix_fmt yuv420p -f mpegts \"{outputFilePath}\"";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

    public async Task<(string stdout, string stdErr)> RenderTimelapseVideoAsync(string videoFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory = Path.GetDirectoryName(videoFilePath) ?? throw new ArgumentException("Invalid directory", nameof(videoFilePath));
        string outputFilePath =
            Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(videoFilePath) + ".tmp" + Path.GetExtension(videoFilePath));
        string arguments = $"-i \"{videoFilePath}\" -filter:v \"setpts=0.5*PTS\" -an \"{outputFilePath}\"";
        return await FfmpegAsync(arguments, workingDirectory, cancellationToken);
    }

}
