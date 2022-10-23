using System.Text;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    // ffmpeg filter attributes
    protected const string DIM_TEXT = "0.8";
    protected const string DIM_BACKGROUND = "0.3";
    protected const string LARGE_FONT = "h/20";
    protected const string SMALL_FONT = "h/35";
    protected const string BORDER_CHANNEL_TEXT = "box=1:boxborderw=10:";
    protected const string BORDER_LOWER_THIRD = "box=1:boxborderw=15:";

    protected const string FILE = "file";
    protected const string NARRATION = "narration";
    protected const string NARRATIVE = "narrative";
    protected const string AUDIO = "audio";

    // ffmpeg positions
    protected readonly string _upperLeft;
    protected readonly string _upperCenter;
    protected readonly string _upperRight;
    protected readonly string _centered;
    protected readonly string _lowerLeft;
    protected readonly string _lowerCenter;
    protected readonly string _lowerRight;

    protected readonly string _subscribeFilter;
    protected readonly string _likeFilter;
    protected readonly Random _random;

    private readonly IFileSystem _fileSystem;
    private readonly IFfmpeg _ffmpeg;
    private readonly ITarball _tarball;
    private readonly AppSettings _appSettings;

    protected BaseVideoService(IFileSystem fileSystem, IFfmpeg ffmpeg, ITarball tarball,
        AppSettings appSettings)
    {
        _fileSystem = fileSystem;
        _ffmpeg = ffmpeg;
        _tarball = tarball;
        _appSettings = appSettings;

        const int PADDING = 30;
        _upperLeft = $"x={PADDING}:y={PADDING}";
        _upperCenter = $"x=(w-tw)/2:y={PADDING}";
        _upperRight = $"x=w-tw-{PADDING}:y={PADDING}";
        _centered = $"x=(w-tw)/2:y=(h-th)/2";
        _lowerLeft = $"x={PADDING}:y=h-th-{PADDING}";
        _lowerCenter = $"x=(w-tw)/2:y=h-th-{PADDING}";
        _lowerRight = $"x=w-tw-{PADDING}:y=h-th-{PADDING}";

        const int CALL_TO_ACTION_DURATION_SECONDS = 5;
        string filterDuration = $"enable=lt(mod(t\\,3)\\,{CALL_TO_ACTION_DURATION_SECONDS})";

        _likeFilter = $"drawtext=text:'GIVE US A THUMBS UP!':fontcolor={FfMpegColors.White}:fontsize={SMALL_FONT}:{_lowerCenter}:boxcolor={FfMpegColors.Blue}:box=1:boxborderw=10:{filterDuration}";
        _subscribeFilter = $"drawtext=text:'SUBSCRIBE FOR FUTURE VIDEOS!':fontcolor={FfMpegColors.White}:fontsize={LARGE_FONT}:{_lowerLeft}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10:{filterDuration}";
        _random = new Random();
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
        if (_appSettings.DeleteFilesAfterDays == 0)
        {
            return;
        }

        DateTime currentDateTime = DateTime.Now;
        var files = _fileSystem.GetFilesInDirectory(directory);
        
        foreach (var file in files)
        {
            if (currentDateTime.Subtract(File.GetLastAccessTime(file)).Days > _appSettings.DeleteFilesAfterDays)
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
                writer.WriteLine($"{FILE} '{file}'");
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
            await _ffmpeg.ConvertVideoFileToTsFormatAsync(
                videoFileName, video.OutputFilePath, cancellationToken);
        }
    }

    internal virtual async Task AddMusicToVideoAsync<T>(T video, CancellationToken cancellationToken) where T : BaseVideo
    {
        var videoFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => !x.Contains(NARRATION) || !x.Contains(NARRATIVE))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = _fileSystem.GetFilesInDirectory(video.WorkingDirectory)
            .Where(x => x.Contains(NARRATION) || x.Contains(NARRATIVE))
            .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
            .ToArray();

        foreach (var videoFilePath in videoFiles)
        {
            var result = await _ffmpeg.FfprobeAsync(
                $"\"{videoFilePath}\"", video.WorkingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains(AUDIO))
            {
                continue;
            }

            string audioFilePath = narrationFiles.Where(
                    x => x.Contains(Path.GetFileNameWithoutExtension(videoFilePath))
                )
                .Single();

            string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFilePath)}.tmp{FileExtension.Mp4}"
                .Replace(NARRATION, string.Empty)
                .Replace(NARRATIVE, string.Empty);

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

            const string RHT_SERVICES_INTRO = "rhtservicesintro.ts";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(RHT_SERVICES_INTRO))
                {
                    continue;
                }

                if (i == 1 && video.Title.ToLower().Contains(Constants.ChristmasLightShow) == false)
                {
                    writer.WriteLine($"{FILE} '{RHT_SERVICES_INTRO}'");
                }

                writer.WriteLine($"{FILE} '{Path.GetFileName(filesInDirectory[i])}'");
            }
        }
    }

    internal async Task CreateTarballsFromDirectoriesAsync(string directory, CancellationToken cancellationToken)
    {
        foreach (var dir in _fileSystem.GetDirectoriesInDirectory(directory))
        {
            await _tarball.CreateTarballFromDirectoryAsync(dir, cancellationToken);
            _fileSystem.DeleteDirectory(dir);
        }
    }

    internal virtual string DrawTextVideoFilter<T>(T video) where T : BaseVideo
    {
        StringBuilder videoFilter = new($"drawtext=textfile:'{video.ChannelBannerText()}':");
        videoFilter.Append($"fontcolor={video.TextColor()}@{DIM_TEXT}:");
        videoFilter.Append($"fontsize={SMALL_FONT}:");
        videoFilter.Append($"{_upperRight}:");
        videoFilter.Append(BORDER_CHANNEL_TEXT);
        videoFilter.Append($"boxcolor={video.BoxColor()}@{DIM_BACKGROUND}");

        return videoFilter.ToString();
    }

    internal virtual void GenerateSubtitleFileWithTitle(string filePath, string title)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("1");
        stringBuilder.Append("00:00:00,000 --> 00:00:03,000");
        stringBuilder.Append(title);

        _fileSystem.SaveFileContents(filePath, stringBuilder.ToString());
    }

    internal virtual void GenerateTitleTextVideo()
    {
        // ffmpeg -y -lavfi "color=green:1920x1080:d=3,subtitles=subtitle.srt:force_style='Alignment=10,OutlineColour=&H100000000,BorderStyle=6,Outline=1,Shadow=1,Fontsize=40,MarginL=5,MarginV=25'" -f matroska output.mp4
    }
}
