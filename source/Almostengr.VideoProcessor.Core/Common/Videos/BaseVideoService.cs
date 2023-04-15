using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Music;
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
    protected readonly IAssSubtitleFileService _assSubtitleFileService;

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
        _assSubtitleFileService = assSubtitleFileService;
    }

    public abstract Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken);
    public abstract Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken);
    // public abstract Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken);
    public abstract Task ProcessVideoProjectAsync(CancellationToken cancellationToken);

    internal async Task CompressTarballsInArchiveFolderAsync(
        string archiveDirectory, CancellationToken cancellationToken)
    {
        var archiveTarballs = _fileSystemService.GetFilesInDirectory(archiveDirectory)
            .Where(f => f.EndsWith(FileExtension.Tar.Value) && !f.Contains(FileExtension.DraftTar.Value));

        foreach (var archive in archiveTarballs)
        {
            await _compressionService.CompressFileAsync(archive, cancellationToken);
        }
    }

    internal IEnumerable<AudioFile> GetAudioFilesInDirectory(string directory)
    {
        return _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.EndsWith(FileExtension.Mp3.Value, StringComparison.OrdinalIgnoreCase))
            .Select(f => new AudioFile(f))
            .ToList();
    }

    internal IEnumerable<string> GetVideoFilesInDirectory(string directory)
    {
        return _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.EndsWith(FileExtension.Mp4.Value, StringComparison.OrdinalIgnoreCase) ||
                f.EndsWith(FileExtension.Mov.Value, StringComparison.OrdinalIgnoreCase) || 
                f.EndsWith(FileExtension.Mkv.Value, StringComparison.OrdinalIgnoreCase));
    }

    internal void CreateFfmpegInputFile(IEnumerable<string> filesInDirectory, string inputFilePath)
    // internal void CreateFfmpegInputFile(string[] filesInDirectory, string inputFilePath)
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
        var directories = _fileSystemService.GetDirectoriesInDirectory(incomingDirectory);
        foreach (var directory in directories)
        {
            if (_fileSystemService.GetFilesInDirectory(directory).Count() == 0)
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
            .Where(f => f.Contains("details.txt", StringComparison.OrdinalIgnoreCase))
            .Any();

        if (fileExists)
        {
            throw new DetailsTxtFileExistsException("details.txt file exists. Please repackage tarball file");
        }
    }

    public void StopProcessingIfFfmpegInputTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.Contains("ffmpeg", StringComparison.OrdinalIgnoreCase))
            .Any();

        if (fileExists)
        {
            throw new OldFfmpegInputFileExistsException("Old ffmpeg input file exists. Please repackage tarball file");
        }
    }

    public void StopProcessingIfKdenliveFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.EndsWith(FileExtension.Kdenlive.Value, StringComparison.OrdinalIgnoreCase))
            .Any();

        if (fileExists)
        {
            throw new KdenliveFileExistsException("Archive has Kdenlive project file. Please repackage tarball file");
        }
    }

    public IEnumerable<string> GetThumbnailFiles(string directory)
    {
         return _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.EndsWith(FileExtension.ThumbTxt.Value, StringComparison.OrdinalIgnoreCase));
    }

}