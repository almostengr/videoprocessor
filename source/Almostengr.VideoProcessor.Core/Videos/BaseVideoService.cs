using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    protected readonly AppSettings _appSettings;
    protected readonly IFfmpegService _ffmpegService;
    protected readonly IFileCompressionService _compressionService;
    protected readonly ITarballService _tarballService;
    protected readonly IFileSystemService _fileSystemService;
    protected readonly IRandomService _randomService;
    protected readonly IMusicService _musicService;

    protected string IncomingDirectory { get; init; }
    protected string ArchiveDirectory { get; init; }
    protected string UploadingDirectory { get; init; }
    protected string WorkingDirectory { get; init; }


    protected BaseVideoService(
        AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        IMusicService musicService, IAssSubtitleFileService assSubtitleFileService)
    {
        _appSettings = appSettings;
        _ffmpegService = ffmpegService;
        _compressionService = gzipService;
        _tarballService = tarballService;
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _musicService = musicService;
    }

    public abstract Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    public abstract Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    public abstract Task ProcessVideoProjectAsync(CancellationToken cancellationToken);

    internal async Task CompressTarballsInArchiveFolderAsync(
        string archiveDirectory, CancellationToken cancellationToken)
    {
        var archiveTarballs = _fileSystemService.GetFilesInDirectory(archiveDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Tar.Value) && !f.ContainsIgnoringCase(FileExtension.DraftTar.Value));

        foreach (var archive in archiveTarballs)
        {
            await _compressionService.CompressFileAsync(archive, cancellationToken);
        }
    }

    internal void CreateFfmpegInputFile(IEnumerable<string> filesInDirectory, string inputFilePath)
    {
        StringBuilder text = new();
        const string FILE = "file";
        foreach (var file in filesInDirectory)
        {
            text.Append($"{FILE} '{file}' {Environment.NewLine}");
        }

        _fileSystemService.SaveFileContents(inputFilePath, text.ToString());
    }

    public async Task CreateTarballsFromDirectoriesAsync(string incomingDirectory, CancellationToken cancellationToken)
    {
        var readyFiles = _fileSystemService.GetFilesInDirectory(incomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
            .ToList();

        foreach(var readyFile in readyFiles)
        {
            string directory = readyFile.Replace(FileExtension.Ready.Value, string.Empty);
            bool hasDirectory = _fileSystemService.DoesDirectoryExist(directory);

            if (!hasDirectory)
            {
                continue;
            }

            await _tarballService.CreateTarballFromDirectoryAsync(directory, cancellationToken);
            _fileSystemService.DeleteDirectory(directory);
        }
    }

    public void StopProcessingIfDetailsTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ContainsIgnoringCase("details.txt"))
            .Any();

        if (fileExists)
        {
            throw new DetailsTxtFileExistsException("details.txt file exists. Please repackage tarball file");
        }
    }

    public void StopProcessingIfFfmpegInputTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ContainsIgnoringCase("ffmpeg"))
            .Any();

        if (fileExists)
        {
            throw new OldFfmpegInputFileExistsException("Old ffmpeg input file exists. Please repackage tarball file");
        }
    }

    public void StopProcessingIfKdenliveFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.EndsWithIgnoringCase(".kdenlive"))
            .Any();

        if (fileExists)
        {
            throw new KdenliveFileExistsException("Archive has Kdenlive project file. Please repackage");
        }
    }

    public IEnumerable<string> GetThumbnailFiles(string directory)
    {
        return _fileSystemService.GetFilesInDirectory(directory)
           .Where(f => f.EndsWithIgnoringCase(FileExtension.ThumbTxt.Value));
    }

    protected async Task AnalyzeAndNormalizeAudioAsync(string audioClipFilePath, CancellationToken cancellationToken)
    {
        var analyzeResult = await _ffmpegService.AnalyzeAudioVolumeAsync(
            audioClipFilePath, cancellationToken);

        var output = analyzeResult.stdErr.Split(Environment.NewLine);
        float maxVolume = float.Parse(output.Where(l => l.Contains("max_volume")).Single().Split(" ")[4]);

        string audioClipFilePathTemp = Path.Combine(WorkingDirectory, "tempaudio" + FileExtension.Mp3.Value);
        var normalizeResult = await _ffmpegService.AdjustAudioVolumeAsync(
            audioClipFilePath, audioClipFilePathTemp, maxVolume, cancellationToken);

        _fileSystemService.MoveFile(audioClipFilePathTemp, audioClipFilePath);
    }
}