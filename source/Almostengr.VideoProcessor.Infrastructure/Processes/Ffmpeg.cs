using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Videos;
using Almostengr.VideoProcessor.Infrastructure.Processes.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.Processes;

public sealed class Ffmpeg : BaseProcess<Ffmpeg>, IFfmpeg
{
    private const string FfmpegBinary = "/usr/bin/ffmpeg";
    private const string FfprobeBinary = "/usr/bin/ffprobe";
    private readonly IFileSystem _fileSystem;

    public Ffmpeg(IFileSystem fileSystemService, ILoggerService<Ffmpeg> loggerService) : 
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

    public async Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken)
    {
        string workingDirectory =
            Path.GetDirectoryName(ffmpegInputFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string arguments =
            $"-init_hw_device vaapi=foo:/dev/dri/renderD128 -hwaccel vaapi -hwaccel_output_format nv12 -f concat -safe 0 -i \"{ffmpegInputFilePath}\" -filter_hw_device foo -vf \"{videoFilter}, format=vaapi|nv12,hwupload\" -vcodec h264_vaapi \"{outputFilePath}\"";

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

    public async Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(videoFilePath) || string.IsNullOrWhiteSpace(outputFilePath))
        {
            throw new InvalidPathException("Invalid paths provided");
        }

        string workingDirectory =
            Path.GetDirectoryName(videoFilePath) ?? throw new ProgramWorkingDirectoryIsInvalidException();
        string outputFileName = 
            Path.Combine(workingDirectory, Path.GetFileNameWithoutExtension(videoFilePath) + FileExtension.Ts);

        return await FfmpegAsync(
            $"-i \"{Path.GetFileName(videoFilePath)}\" -c copy -bsf:v h264_mp4toannexb -f mpegts \"{outputFileName}\"",
            workingDirectory,
            cancellationToken
        );
    }

    public async Task<(string stdout, string stdErr)> CreateThumbnailsFromVideoFilesAsync<T>(
        T video, CancellationToken cancellationToken) where T : BaseVideo
    {
        const int SCENE_CHANGE_PERCENT = 10;
        const int EXTRACT_NUMBER_OF_FRAMES = 5;

        var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory);

        for (int i = 0; i < videoFiles.Count(); i++)
        {
            await FfmpegAsync(
               $"-i \"{videoFiles.ElementAt(i)}\" -vf select=gt(scene\\,0.{SCENE_CHANGE_PERCENT}) -frames:v {EXTRACT_NUMBER_OF_FRAMES} -vsync vfr \"{video.Title}.{i}.%03d.jpg\"",
               video.WorkingDirectory,
               cancellationToken
           );
        }

        return (string.Empty, string.Empty);
    }

    public async Task<(string stdout, string stdErr)> CreateTitleTextVideoAsync<T>(
        T video, CancellationToken cancellationToken) where T : BaseVideo
    {
        // ffmpeg -y -lavfi "color=green:1920x1080:d=3,subtitles=subtitle.srt:force_style='Alignment=10,OutlineColour=&H100000000,BorderStyle=6,Outline=1,Shadow=1,Fontsize=40,MarginL=5,MarginV=25'"
        return await FfmpegAsync(
            $"-y -lavfi \"color={video.BoxColor()}:1920x1080:d=3,subtitles=title.srt:force_style='Alignment=10,OutlineColour=&H100000000,BorderStyle=6,Outline=1,Shadow=1,Fontsize=40,MarginL=5,MarginV=25'\"",
            video.WorkingDirectory,
            cancellationToken
        );
    }
}
