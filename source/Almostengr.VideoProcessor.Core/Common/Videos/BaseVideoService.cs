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
    protected readonly IAssSubtitleFileService _assSubtitleFileService;

    protected string IncomingDirectory { get; init; }
    protected string ArchiveDirectory { get; init; }
    protected string UploadingDirectory { get; init; }
    // protected string ReviewingDirectory { get; set; }
    // protected string ReviewWorkDirectory { get; init; }
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
    public abstract Task ProcessIncomingTarballFilesAsync(CancellationToken cancellationToken);
    // public virtual Task ProcessReviewedFilesAsync(CancellationToken cancellationToken)
    // {
    //     throw new not 
    // }
    // public abstract Task ProcessIncomingSubtitlesAsync(CancellationToken cancellationToken);

    internal string RandomChannelBrandingText(string[] options)
    {
        return options.ElementAt(_randomService.Next(0, options.Count()));
    }

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

    internal void CreateFfmpegInputFile(string[] filesInDirectory, string inputFilePath)
    {
        StringBuilder text = new();
        const string FILE = "file";
        foreach (var file in filesInDirectory)
        {
            text.Append($"{FILE} '{file}' {Environment.NewLine}");
        }

        _fileSystemService.SaveFileContents(inputFilePath, text.ToString());
    }

    protected async Task ConvertVideoAudioFilesToAudioOnly(
        string workingDirectory, CancellationToken cancellationToken)
    {
        var audioAsVideoFiles = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(f => f.EndsWith(FileExtension.AudioMkv.Value) || f.EndsWith(FileExtension.AudioMp4.Value));

        foreach (var file in audioAsVideoFiles)
        {
            string outputFilePath = Path.Combine(workingDirectory,
                file.Replace(FileExtension.AudioMkv.Value, FileExtension.Mp3.Value)
                    .Replace(FileExtension.AudioMp4.Value, FileExtension.Mp3.Value));

            await _ffmpegService.ConvertVideoFileToMp3FileAsync(
                file, outputFilePath, workingDirectory, cancellationToken);
        }
    }

    public async Task CreateTarballsFromDirectoriesAsync(string incomingDirectory, CancellationToken cancellationToken)
    {
        var directories = _fileSystemService.GetDirectoriesInDirectory(incomingDirectory);
        foreach (var directory in directories)
        {
            await _tarballService.CreateTarballFromDirectoryAsync(directory, cancellationToken);
            _fileSystemService.DeleteDirectory(directory);
        }
    }

    public void StopProcessingIfDetailsTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ToLower().Contains("details.txt"))
            .Any();

        if (fileExists)
        {
            throw new DetailsTxtFileExistsException("details.txt file exists. Please repackage tarball file");
        }
    }

    public void StopProcessingIfFfmpegInputTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Kdenlive.Value))
            .Any();

        if (fileExists)
        {
            throw new OldFfmpegInputFileExistsException("Old ffmpeg input file exists. Please repackage tarball file");
        }
    }

    public void StopProcessingIfKdenliveFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ToLower().EndsWith(FileExtension.Kdenlive.Value))
            .Any();

        if (fileExists)
        {
            throw new KdenliveFileExistsException("Archive has Kdenlive project file. Please repackage tarball file");
        }
    }

}