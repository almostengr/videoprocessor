using System.Text;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    // internal const int RHT_BORDER_WIDTH = 7;
    private const int PADDING = 30;

    // essential files
    // protected const string FFMPEG_INPUT_FILE = "ffmpeginput.txt";
    // protected const string SUBTITLES_FILE = "subtitles.ass";

    // ffmpeg options
    // protected const string LOG_ERRORS = "-loglevel error";
    // protected const string LOG_WARNINGS = "-loglevel warning";
    // protected const string HW_OPTIONS = "-hide_banner -y -hwaccel vaapi -hwaccel_output_format vaapi";
    // protected const string HW_VCODEC = "-vcodec h264_vaapi -b:v 5M";

    // ffmpeg filter attributes
    // protected const int DASHCAM_BORDER_WIDTH = 10;
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

    protected readonly string _subscribeFilter;
    // protected readonly string _subscribeScrollingFilter;
    protected readonly string _likeFilter;
    // protected readonly string _facebookFilter;
    // protected readonly string _instagramFilter;
    // protected readonly string _youtubeFilter;

    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;

    protected BaseVideoService(IFileSystem fileSystem, IFfmpeg ffmpeg, ITarball tarball)
    {
        _fileSystem = fileSystem;
        _ffmpeg = ffmpeg;
        _tarball = tarball;

        _upperLeft = $"x={PADDING}:y={PADDING}";
        _upperCenter = $"x=(w-tw)/2:y={PADDING}";
        _upperRight = $"x=w-tw-{PADDING}:y={PADDING}";
        _centered = $"x=(w-tw)/2:y=(h-th)/2";
        _lowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
        _lowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
        _lowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";

        const int callToActionDurationSeconds = 5;
        string filterDuration = $"enable=lt(mod(t\\,3)\\,{callToActionDurationSeconds})";

        _likeFilter = $"drawtext=text:'GIVE US A THUMBS UP!':fontcolor={FfMpegColors.White}:fontsize={SMALL_FONT}:{_lowerCenter}:boxcolor={FfMpegColors.Blue}:box=1:boxborderw=10:{filterDuration}";
        _subscribeFilter = $"drawtext=text:'SUBSCRIBE FOR FUTURE VIDEOS!':fontcolor={FfMpegColors.White}:fontsize={LARGE_FONT}:{_lowerLeft}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10:{filterDuration}";
        // _subscribeScrollingFilter = $"drawtext=text:'SUBSCRIBE':fontcolor={FfMpegColors.White}:fontsize={SMALL_FONT}:x=w+(100*t):y=h-th-{PADDING}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10";
    }

    internal void CreateVideoDirectories<T>(T video) where T : BaseVideo
    {
        _fileSystem.CreateDirectory(video.IncomingDirectory);
        _fileSystem.CreateDirectory(video.ArchiveDirectory);
        _fileSystem.CreateDirectory(video.UploadDirectory);
        _fileSystem.CreateDirectory(video.WorkingDirectory);
    }

    public abstract Task ProcessVideosAsync(CancellationToken stoppingToken);

    internal void DeleteFilesOlderThanSpecifiedDays(string directory)
    {
        const int SPECIFIED_DAYS = 14;
        DateTime currentDateTime = DateTime.Now;

        var files = _fileSystem.GetFilesInDirectory(directory);
        foreach(var file in files)
        {
            if (currentDateTime.Subtract(File.GetLastAccessTime(file)).Days > SPECIFIED_DAYS)
            {
                _fileSystem.DeleteFile(file);
            }
        }
    }
    
    internal virtual void CreateFfmpegInputFile<T>(T video) where T : BaseVideo
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                .Where(f => f.EndsWith(FileExtension.Ts))
                .OrderBy(f => f)
                .ToArray();

            foreach (var file in filesInDirectory)
            {
                writer.WriteLine($"file '{file}'");
            }
        }
    }

    internal virtual async Task ConvertImagesToVideo(string directory, CancellationToken cancellationToken)
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

            await _ffmpeg.ImagesToVideoAsync(image, outputFile, cancellationToken);
        }
    }

    internal virtual async Task ConvertVideoFilesToCommonFormatAsync<T>(T video, CancellationToken cancellationToken) where T : BaseVideo
    {
        var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mp4))
            .OrderBy(x => x)
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            // var result = await _ffmpeg.FfprobeAsync(videoFileName, video.WorkingDirectory, cancellationToken);

            // string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{video.xResolution}x{video.yResolution}{FileExtension.Mp4}";

            // if (result.stdErr.Contains($"{video.xResolution}x{video.yResolution}") &&
            //     result.stdErr.Contains($"{video.audioBitRate} Hz") &&
            //     result.stdErr.Contains($"196 kb/s") &&
            //     videoFileName.EndsWith(FileExtension.Mp4))
            // {
            //     _fileSystem.MoveFile(
            //         Path.Combine(video.WorkingDirectory, videoFileName),
            //         Path.Combine(video.WorkingDirectory, scaledFile),
            //         false);
            //     continue;
            // }

            // await _ffmpeg.FfmpegAsync(
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={video.xResolution}:h={video.yResolution}\" -vcodec h264_vaapi -ar {video.audioSampleRate} -b:a {video.audioBitRate} \"{scaledFile}\"",
            //     video.WorkingDirectory,
            //     cancellationToken);

            await _ffmpeg.ConvertVideoFileToTsFormatAsync(
                videoFileName, video.OutputFilePath, cancellationToken);

            // _fileSystem.DeleteFile(Path.Combine(video.WorkingDirectory, videoFileName));

            // string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
        }
    }

    internal virtual async Task AddMusicToVideoAsync<T>(T video, CancellationToken cancellationToken) where T : BaseVideo
    {
        const string narration = "narration";
        const string narrative = "narrative";
        const string audio = "audio";

        var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => !x.Contains(narration) || !x.Contains(narrative))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => x.Contains(narration) || x.Contains(narrative))
            .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
            .ToArray();

        foreach (var videoFilePath in videoFiles)
        {
            // var result = await RunCommandAsync(
            //     ProgramPaths.Ffprobe,
            //     $"-hide_banner \"{videoFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     1
            // );

            var result = await _ffmpeg.FfprobeAsync(
                $"\"{videoFilePath}\"", video.WorkingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains(audio))
            {
                continue;
            }

            string audioFilePath = narrationFiles.Where(
                    x => x.Contains(Path.GetFileNameWithoutExtension(videoFilePath))
                )
                .Single();

            string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFilePath)}.tmp{FileExtension.Mp4}"
                .Replace(narration, string.Empty)
                .Replace(narrative, string.Empty);

            // await RunCommandAsync(
            //     ProgramPaths.Ffmpeg,
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
            //     workingDirectory,
            //     cancellationToken,
            //     10);

            // await _ffmpeg.FfmpegAsync(
            //     $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFilePath)}\" -i \"{audioFilePath}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
            //     video.WorkingDirectory,
            //     cancellationToken
            // );
            await _ffmpeg.AddAudioToVideoAsync(
                videoFilePath, audioFilePath, video.OutputFilePath, cancellationToken);

            _fileSystem.DeleteFile(videoFilePath);
            _fileSystem.MoveFile(Path.Combine(video.WorkingDirectory, outputFileName), videoFilePath);
        }

        _fileSystem.DeleteFiles(narrationFiles);
    }

    internal void RhtCreateFfmpegInputFile<T>(T video) where T : BaseVideo
    {
        _fileSystem.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
                .Where(x => x.EndsWith(FileExtension.Ts))
                .OrderBy(x => x)
                .ToArray();

            const string rhtservicesintro = "rhtservicesintro.ts";
            const string file = "file";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(rhtservicesintro))
                {
                    continue;
                }

                if (i == 1 && video.Title.ToLower().Contains(Constants.ChristmasLightShow) == false)
                {
                    writer.WriteLine($"{file} '{rhtservicesintro}'");
                }

                writer.WriteLine($"{file} '{Path.GetFileName(filesInDirectory[i])}'");
            }
        }
    }

    internal async Task CreateTarballsFromDirectoriesAsync(string directory, CancellationToken cancellationToken)
    {
        foreach (var dir in _fileSystem.GetDirectoriesInDirectory(directory))
        {
            await _tarball.CreateTarballFromDirectoryAsync(dir, cancellationToken);
            _fileSystem.DeleteDirectory(directory);
        }
    }

    internal virtual string DrawTextVideoFilter<T>(T video) where T : BaseVideo
    {
        StringBuilder videoFilter = new();
        videoFilter.Append($"drawtext=textfile:'{video.ChannelBannerText()}':");
        videoFilter.Append($"fontcolor={video.TextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={LARGE_FONT}:");
        videoFilter.Append($"{_upperRight}");
        videoFilter.Append($"box=1:");
        videoFilter.Append($"boxborderw=10:");
        videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

        return videoFilter.ToString();
    }

}
