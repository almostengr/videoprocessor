using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    internal const int RHT_BORDER_WIDTH = 7;
    private const int PADDING = 30;

    // essential files
    // protected const string FFMPEG_INPUT_FILE = "ffmpeginput.txt";
    // protected const string SUBTITLES_FILE = "subtitles.ass";

    // ffmpeg options
    protected const string LOG_ERRORS = "-loglevel error";
    // protected const string LOG_WARNINGS = "-loglevel warning";
    protected const string HW_OPTIONS = "-hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi";
    protected const string HW_VCODEC = "-vcodec h264_vaapi -b:v 5M";

    // ffmpeg filter attributes
    protected const int DASHCAM_BORDER_WIDTH = 10;
    protected const string DIM_TEXT = "0.8";
    protected const string DIM_BACKGROUND = "0.3";
    protected const string LARGE_FONT = "h/20";
    protected const string SMALL_FONT = "h/35";

    // ffmpeg positions
    protected readonly string _upperLeft;
    protected readonly string _upperCenter;
    protected readonly string _upperRight;
    protected readonly string _centered;
    protected readonly string _lowerLeft;
    protected readonly string _lowerCenter;
    protected readonly string _lowerRight;

    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;

    protected BaseVideoService(IFileSystem fileSystemService, IFfmpeg ffmpegService)
    {
        _fileSystem = fileSystemService;
        _ffmpeg = ffmpegService;

        _upperLeft = $"x={PADDING}:y={PADDING}";
        _upperCenter = $"x=(w-tw)/2:y={PADDING}";
        _upperRight = $"x=w-tw-{PADDING}:y={PADDING}";
        _centered = $"x=(w-tw)/2:y=(h-th)/2";
        _lowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
        _lowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
        _lowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";
    }

    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
    internal abstract string FfmpegVideoFilter<T>(T video) where T : BaseVideo;

    internal virtual void CreateFfmpegInputFile<T>(T video) where T : BaseVideo
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Mp4))
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in filesInDirectory)
            {
                writer.WriteLine($"file '{file}'");
            }
        }
    }

    protected virtual async Task ConvertImagesToVideo(string directory, CancellationToken cancellationToken)
    {
        var imageFiles = _fileSystem.GetFilesInDirectory(directory)
            .Where(x => x.EndsWith(FileExtension.Jpg) || x.EndsWith(FileExtension.Png))
            .Where(x => x.StartsWith(".") == false);

        foreach (var image in imageFiles)
        {
            string outputFile = Path.GetFileNameWithoutExtension(image) + FileExtension.Mp4;

            // await _ffmpeg.FfmpegAsync(
            //     $"{LOG_ERRORS} -framerate 1/{duration} -i \"{image}\" -c:v libx264 -t {duration} \"{outputFile}\"",
            //     directory,
            //     cancellationToken
            // );

            await _ffmpeg.ImagesToVideoAsync(
                image, outputFile, directory, cancellationToken);
        }
    }
}