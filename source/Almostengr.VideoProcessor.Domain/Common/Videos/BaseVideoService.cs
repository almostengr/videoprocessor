using System.Text;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    protected const string FILE = "file";
    protected const string NARRATION = "narration";
    protected const string NARRATIVE = "narrative";
    protected const string AUDIO = "audio";

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
    }

    internal void CreateVideoDirectories<T>(T video) where T : BaseVideo
    {
        _fileSystem.CreateDirectory(video.IncomingDirectory);
        _fileSystem.CreateDirectory(video.ArchiveDirectory);
        _fileSystem.CreateDirectory(video.UploadDirectory);
        _fileSystem.CreateDirectory(video.WorkingDirectory);
        _fileSystem.CreateDirectory(video.ErrorDirectory);
    }

    public abstract Task<bool> ProcessVideoAsync(CancellationToken stoppingToken);
    internal abstract string SelectChannelBannerText();

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
            .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mp4) || 
                x.EndsWith(FileExtension.Avi) || x.EndsWith(FileExtension.Mov));

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

    internal async Task CreateTarballsFromDirectoriesAsync(string directory, CancellationToken cancellationToken)
    {
        foreach (var dir in _fileSystem.GetDirectoriesInDirectory(directory))
        {
            await _tarball.CreateTarballFromDirectoryAsync(dir, cancellationToken);
            _fileSystem.DeleteDirectory(dir);
        }
    }

    internal virtual void GenerateSubtitleFileWithTitle(string filePath, string title)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append("1");
        stringBuilder.Append("00:00:00,000 --> 00:00:03,000");
        stringBuilder.Append(title);

        _fileSystem.SaveFileContents(filePath, stringBuilder.ToString());
    }

    protected string[] RhtServicesBannerTextOptions()
    {
        return new string[] {
            "rhtservices.net",
            "Robinson Handy and Technology Services",
            "rhtservices.net/courses",
            "rhtservices.net/facebook",
            "rhtservices.net/instagram",
            // "rhtservices.net/youtube",
            "@rhtservicesllc"
            };
    }
}
