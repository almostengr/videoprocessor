namespace Almostengr.VideoProcessor.Application.Video;

public abstract class BaseVideoService : IBaseVideoService
{
    private const int PADDING = 30;

    // essential files
    protected const string FFMPEG_INPUT_FILE = "ffmpeginput.txt";
    protected const string SUBTITLES_FILE = "subtitles.ass";

    // ffmpeg options
    protected const string LOG_ERRORS = "-loglevel error";
    protected const string LOG_WARNINGS = "-loglevel warning";
    protected const string HW_OPTIONS = "-hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi";
    protected const string HW_VCODEC = "-vcodec h264_vaapi -b:v 5M";

    // ffmpeg filter attributes
    protected const int DASHCAM_BORDER_WIDTH = 10;
    protected const string DIM_TEXT = "0.8";
    protected const string DIM_BACKGROUND = "0.3";
    protected const string LARGE_FONT = "h/20";
    protected const string SMALL_FONT = "h/35";
    protected const int RHT_BORDER_WIDTH = 7;



    // ffmpeg positions
    protected readonly string _upperLeft;
    protected readonly string _upperCenter;
    protected readonly string _upperRight;
    protected readonly string _centered;
    protected readonly string _lowerLeft;
    protected readonly string _lowerCenter;
    protected readonly string _lowerRight;

    protected BaseVideoService()
    {
        _upperLeft = $"x={PADDING}:y={PADDING}";
        _upperCenter = $"x=(w-tw)/2:y={PADDING}";
        _upperRight = $"x=w-tw-{PADDING}:y={PADDING}";
        _centered = $"x=(w-tw)/2:y=(h-th)/2";
        _lowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
        _lowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
        _lowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";
    }

    public abstract Task ExecuteAsync(CancellationToken stoppingToken);

    protected virtual async Task ConvertImagesToVideo(string directory, CancellationToken cancellationToken)
    {
        var imageFiles = GetFilesInDirectory(directory)
            .Where(x => x.EndsWith(FileExtension.Jpg) || x.EndsWith(FileExtension.Png))
            .Where(x => x.StartsWith(".") == false);

        foreach (var image in imageFiles)
        {
            string outputFile = Path.GetFileNameWithoutExtension(image) + FileExtension.Mp4;
            int duration = 3;

            // ffmpeg -framerate 1/10 -i DJI_0024.JPG -c:v libx264 -t 10 -pix_fmt yuv420p -vf scale=320:240 out.mp4
            // ffmpeg -loop 1 -i image.png -c:v libx264 -t 15 -pix_fmt yuv420p -vf scale=320:240 out.mp4

            await RunCommandAsync(
                ProgramPaths.Ffmpeg,
                $"{LOG_ERRORS} -framerate 1/{duration} -i \"{image}\" -c:v libx264 -t {duration} \"{outputFile}\"",
                directory,
                cancellationToken,
                1
            );
        }
    }
}