using System.Text;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.Videos;

public sealed class ProcessVideo : IProcessVideo
{
    private readonly AppSettings _appSettings;
    private readonly IFfmpegService _ffmpegService;
    private readonly IFileCompressionService _compressionService;
    private readonly ITarballService _tarballService;
    private readonly IFileSystemService _fileSystemService;
    private readonly IRandomService _randomService;
    private readonly IGzFileCompressionService _gzipService;
    private readonly ILoggerService<ProcessVideo> _loggerService;
    private readonly IMusicService _musicService;
    private readonly string _incomingDirectory;
    private readonly string _workingDirectory;

    public ProcessVideo(AppSettings appSettings, IFfmpegService ffmpegService,
    IFileCompressionService compressionService,
    ITarballService tarballService, IFileSystemService fileSystemService,
    IRandomService randomService, IGzFileCompressionService gzipService,
    ILoggerService<ProcessVideo> loggerService, IMusicService musicService)
    {
        _appSettings = appSettings;
        _ffmpegService = ffmpegService;
        this._compressionService = compressionService;
        _tarballService = tarballService;
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _gzipService = gzipService;
        _loggerService = loggerService;
        _musicService = musicService;
        _incomingDirectory = _appSettings.IncomingVideoDirectory;
        _workingDirectory = _appSettings.WorkingVideoDirectory;
    }

    public async Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken) { }
    public async Task ProcessVideoProjectAsync(CancellationToken cancellationToken) { }
    
    public async Task RenderVideo(CancellationToken cancellationToken)
    {
        string? readyFile = _fileSystemService.GetFilesInDirectory(_incomingDirectory)
                    .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
                    .FirstOrDefault();

        if (readyFile == null)
        {
            return;
        }

        string projectFileName =
            Path.GetFileName(readyFile.ReplaceIgnoringCase(FileExtension.Ready.Value, FileExtension.Tar.Value));

        IVideoProject project = GetProjectType(projectFileName);

        try
        {
            _fileSystemService.DeleteDirectory(_workingDirectory);
            _fileSystemService.CreateDirectory(_workingDirectory);

            await _tarballService.ExtractTarballContentsAsync(
                project.FilePath(), _workingDirectory, cancellationToken);

            StopProcessingIfDetailsTxtFileExists(_workingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(_workingDirectory);
            StopProcessingIfKdenliveFileExists(_workingDirectory);

            var videoClips = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.Mp4.Value))
                .OrderBy(f => f);

            string ffmpegInputFilePath = Path.Combine(_workingDirectory, Constant.FfmpegInputFileName);
            CreateFfmpegInputFile(videoClips, ffmpegInputFilePath);

            var textOptions = project.BrandingTextOptions().ToList();
            string brandingText = textOptions[_randomService.Next(0, textOptions.Count)];
            string outputFilePath = Path.Combine(_workingDirectory, project.VideoFileName());

            await _ffmpegService.RenderVideoWithInputFileAndFiltersAsync(
                ffmpegInputFilePath, project.ChannelBrandDrawTextFilter(brandingText), outputFilePath, cancellationToken);

            _fileSystemService.MoveFile(outputFilePath, Path.Combine(project.UploadDirectory(), project.VideoFileName()));
            _fileSystemService.MoveFile(
                project.FilePath(), Path.Combine(project.ArchiveDirectory(), project.FileName()));
            _fileSystemService.DeleteFile(readyFile);
            _fileSystemService.DeleteDirectory(_workingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(project.FilePath(), ex);
            _fileSystemService.MoveFile(readyFile, readyFile + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(_workingDirectory);
        }
    }

    private IVideoProject GetProjectType(string projectFileName)
    {
        if (projectFileName.ContainsIgnoringCase(FileExtension.DashCamTar.Value) ||
            projectFileName.ContainsIgnoringCase(FileExtension.AutoRepairTar.Value) ||
            projectFileName.ContainsIgnoringCase(FileExtension.FireworksTar.Value)
            )
        {
            return new DashCamVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }
        else if (projectFileName.ContainsIgnoringCase(FileExtension.HandymanTar.Value))
        {
            return new HandymanVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }
        else if (projectFileName.ContainsIgnoringCase(FileExtension.TechnologyTar.Value))
        {
            return new TechTalkVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }
        else if (projectFileName.ContainsIgnoringCase(FileExtension.ToastmastersTar.Value))
        {
            return new ToastmastersVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }

        throw new ArgumentException("Unable to determine video type", nameof(projectFileName));
    }


    private async Task CompressTarballsInArchiveFolderAsync(
        string archiveDirectory, CancellationToken cancellationToken)
    {
        var archiveTarballs = _fileSystemService.GetFilesInDirectory(archiveDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Tar.Value) && !f.ContainsIgnoringCase(FileExtension.DraftTar.Value));

        foreach (var archive in archiveTarballs)
        {
            await _compressionService.CompressFileAsync(archive, cancellationToken);
        }
    }

    private void CreateFfmpegInputFile(IEnumerable<string> filesInDirectory, string inputFilePath)
    {
        StringBuilder text = new();
        const string FILE = "file";
        foreach (var file in filesInDirectory)
        {
            text.Append($"{FILE} '{file}' {Environment.NewLine}");
        }

        _fileSystemService.SaveFileContents(inputFilePath, text.ToString());
    }

    private async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        var readyFiles = _fileSystemService.GetFilesInDirectory(_incomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
            .ToList();

        foreach (var readyFile in readyFiles)
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

    private void StopProcessingIfDetailsTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ContainsIgnoringCase("details.txt"))
            .Any();

        if (fileExists)
        {
            throw new DetailsTxtFileExistsException("details.txt file exists. Please repackage tarball file");
        }
    }

    private void StopProcessingIfFfmpegInputTxtFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.ContainsIgnoringCase("ffmpeg"))
            .Any();

        if (fileExists)
        {
            throw new OldFfmpegInputFileExistsException("Old ffmpeg input file exists. Please repackage tarball file");
        }
    }

    private void StopProcessingIfKdenliveFileExists(string directory)
    {
        bool fileExists = _fileSystemService.GetFilesInDirectory(directory)
            .Where(f => f.EndsWithIgnoringCase(".kdenlive"))
            .Any();

        if (fileExists)
        {
            throw new KdenliveFileExistsException("Archive has Kdenlive project file. Please repackage");
        }
    }

    protected async Task AnalyzeAndNormalizeAudioAsync(string audioClipFilePath, CancellationToken cancellationToken)
    {
        var analyzeResult = await _ffmpegService.AnalyzeAudioVolumeAsync(
            audioClipFilePath, cancellationToken);

        var output = analyzeResult.stdErr.Split(Environment.NewLine);
        float maxVolume = float.Parse(output.Where(l => l.Contains("max_volume")).Single().Split(" ")[4]);

        string audioClipFilePathTemp = Path.Combine(_workingDirectory, "tempaudio" + FileExtension.Mp3.Value);
        var normalizeResult = await _ffmpegService.AdjustAudioVolumeAsync(
            audioClipFilePath, audioClipFilePathTemp, maxVolume, cancellationToken);

        _fileSystemService.MoveFile(audioClipFilePathTemp, audioClipFilePath);
    }
}