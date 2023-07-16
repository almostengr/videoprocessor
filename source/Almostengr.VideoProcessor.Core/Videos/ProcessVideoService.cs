using System.Text;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.Videos;

public sealed class ProcessVideoService : IProcessVideoService
{
    private readonly AppSettings _appSettings;
    private readonly IFfmpegService _ffmpegService;
    private readonly IFileCompressionService _compressionService;
    private readonly ITarballService _tarballService;
    private readonly IFileSystemService _fileSystemService;
    private readonly IRandomService _randomService;
    private readonly IGzFileCompressionService _gzipService;
    private readonly ILoggerService<ProcessVideoService> _loggerService;
    private readonly IMusicService _musicService;
    private readonly string _incomingDirectory;
    private readonly string _workingDirectory;
    private readonly string _ffmpegInputFilePath;
    private readonly ICsvGraphicsFileService _csvGraphicsFileService;

    public ProcessVideoService(AppSettings appSettings, IFfmpegService ffmpegService,
    IFileCompressionService compressionService, ICsvGraphicsFileService csvGraphicsFileService,
    ITarballService tarballService, IFileSystemService fileSystemService,
    IRandomService randomService, IGzFileCompressionService gzipService,
    ILoggerService<ProcessVideoService> loggerService, IMusicService musicService)
    {
        _appSettings = appSettings;
        _ffmpegService = ffmpegService;
        _compressionService = compressionService;
        _tarballService = tarballService;
        _fileSystemService = fileSystemService;
        _randomService = randomService;
        _gzipService = gzipService;
        _loggerService = loggerService;
        _musicService = musicService;
        _incomingDirectory = _appSettings.IncomingVideoDirectory;
        _workingDirectory = _appSettings.WorkingVideoDirectory;
        _ffmpegInputFilePath = Path.Combine(_workingDirectory, "video.ffmpeginput");
        _csvGraphicsFileService = csvGraphicsFileService;
    }

    public async Task<bool> RenderVideoAsync(CancellationToken cancellationToken)
    {
        _fileSystemService.CreateDirectory(_incomingDirectory);

        string? readyFile = _fileSystemService.GetFilesInDirectory(_incomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
            .FirstOrDefault();

        if (readyFile == null)
        {
            return false;
        }

        string projectFileName =
            Path.GetFileName(readyFile.ReplaceIgnoringCase(FileExtension.Ready.Value, FileExtension.Tar.Value));

        try
        {
            string tarballFileName = _fileSystemService.GetFilesInDirectory(_incomingDirectory)
               .Where(f => f.ContainsIgnoringCase(projectFileName))
               .Single();

            IVideoProject project = GetProjectType(tarballFileName);

            _fileSystemService.DeleteDirectory(_workingDirectory);
            _fileSystemService.CreateDirectory(_workingDirectory);

            _fileSystemService.CreateDirectory(project.UploadDirectory());
            _fileSystemService.CreateDirectory(project.ArchiveDirectory());

            await _tarballService.ExtractTarballContentsAsync(
                project.FilePath(), _workingDirectory, cancellationToken);

            StopProcessingIfDetailsTxtFileExists(_workingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(_workingDirectory);
            StopProcessingIfKdenliveFileExists(_workingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(_workingDirectory);

            var textOptions = project.BrandingTextOptions();
            string brandingText = textOptions[_randomService.Next(0, textOptions.Count)];
            StringBuilder videoFilters = new(project.ChannelBrandDrawTextFilter(brandingText));

            var videoClips = _fileSystemService.GetFilesInDirectoryWithFileInfo(_workingDirectory)
                .Where(f => f.FullName.IsVideoFile())
                .OrderBy(f => f.Name)
                .ToList();

            foreach (var videoClip in videoClips)
            {
                await NormalizeAudioAndAddGraphicsToVideoClipAsync(project, brandingText, videoClip, cancellationToken);
            }

            var tsVideoFiles = _fileSystemService.GetFilesInDirectory(_workingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.Ts.Value))
                .OrderBy(f => f)
                .ToList();

            if (tsVideoFiles.Count() == 0)
            {
                tsVideoFiles = videoClips.Select(f => f.FullName).ToList();
            }

            CreateFfmpegInputFile(tsVideoFiles, _ffmpegInputFilePath);

            string outputFilePath = Path.Combine(_workingDirectory, project.OutputFileName());

            if (project.GetType() == typeof(DashCamVideoProject) ||
                project.GetType() == typeof(NightTimeDashCamVideoProject))
            {
                var audioFile = _musicService.GetRandomMixTrack();
                await _ffmpegService.RenderVideoWithAudioAsync(
                    _ffmpegInputFilePath, audioFile.FilePath, outputFilePath, cancellationToken);
            }
            else
            {
                await _ffmpegService.RenderVideoAsync(_ffmpegInputFilePath, outputFilePath, cancellationToken);
            }

            _fileSystemService.MoveFile(outputFilePath, project.UploadFilePath());
            _fileSystemService.MoveFile(project.FilePath(), project.ArchiveFilePath());
            _fileSystemService.DeleteFile(readyFile);
            _fileSystemService.DeleteDirectory(_workingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(projectFileName, ex);
            _fileSystemService.MoveFile(readyFile, readyFile + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(_workingDirectory);
        }

        return true;
    }

    private async Task NormalizeAudioAndAddGraphicsToVideoClipAsync(
        IVideoProject project, string brandingText, FileInfo videoClip, CancellationToken cancellationToken)
    {
        var result = await _ffmpegService.FfprobeAsync($"\"{videoClip.FullName}\"", _workingDirectory, cancellationToken);
        string audioClipFilePath =
            Path.Combine(_workingDirectory, Path.GetFileNameWithoutExtension(videoClip.Name) + FileExtension.Mp3.Value);

        if (result.stdErr.DoesNotContainIgnoringCase(Constant.Audio))
        {
            audioClipFilePath = _musicService.GetRandomMixTrack().FilePath;
        }
        else
        {
            var audioConversionResult = await _ffmpegService.ConvertVideoFileToMp3FileAsync(
                videoClip.FullName, audioClipFilePath, _workingDirectory, cancellationToken);

            var analyzeResult =
                await _ffmpegService.AnalyzeAudioVolumeAsync(audioClipFilePath, cancellationToken);

            var output = analyzeResult.stdErr.Split(Environment.NewLine);
            float maxVolume = float.Parse(output.Where(l => l.Contains("max_volume")).Single().Split(" ")[4]);

            string audioClipFilePathTemp = Path.Combine(_workingDirectory, "tempaudio" + FileExtension.Mp3.Value);
            var normalizeResult = await _ffmpegService.AdjustAudioVolumeAsync(
                audioClipFilePath, audioClipFilePathTemp, maxVolume, cancellationToken);

            _fileSystemService.MoveFile(audioClipFilePathTemp, audioClipFilePath);
        }

        string tsFilePath =
            Path.Combine(_workingDirectory, Path.GetFileNameWithoutExtension(videoClip.Name) + FileExtension.Ts.Value);

        StringBuilder videoFilter = new();
        videoFilter.Append(project.ChannelBrandDrawTextFilter(brandingText));

        string? lowerThirdFile = _fileSystemService.GetFilesInDirectory(_workingDirectory)
            .Where(f => f.EndsWithIgnoringCase(Path.GetFileNameWithoutExtension(videoClip.FullName) + ".subtitle.csv") ||
                f.EndsWithIgnoringCase(Path.GetFileNameWithoutExtension(videoClip.FullName) + FileExtension.StreetsCsv.Value))
            .SingleOrDefault();

        if (!string.IsNullOrEmpty(lowerThirdFile))
        {
            var graphicsContent = _csvGraphicsFileService.ReadFile(lowerThirdFile);

            foreach (var graphic in graphicsContent)
            {
                if (string.IsNullOrEmpty(graphic.Text))
                {
                    continue;
                }

                videoFilter.Append(Constant.CommaSpace);

                var startTime = graphic.StartTime;
                var endTime = startTime.Add(TimeSpan.FromSeconds(5));
                videoFilter.Append(
                    new DrawTextFilter(
                        graphic.Text,
                        project.DrawTextFilterBackgroundColor(),
                        Opacity.Full,
                        project.DrawTextFilterBackgroundColor(),
                        Opacity.Full,
                        DrawTextPosition.SubtitlePrimary,
                        startTime,
                        endTime).ToString());
            }
        }

        await _ffmpegService.RenderVideoWithAudioAndFiltersAsync(
            videoClip.FullName, audioClipFilePath, videoFilter.ToString(), tsFilePath, cancellationToken);
    }


    public async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        _fileSystemService.CreateDirectory(_incomingDirectory);

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


    private IVideoProject GetProjectType(string projectFileName)
    {
        if (projectFileName.ContainsIgnoringCase(FileExtension.DashCamTar.Value) ||
            projectFileName.ContainsIgnoringCase(FileExtension.AutoRepairTar.Value) ||
            projectFileName.ContainsIgnoringCase(FileExtension.FireworksTar.Value)
            )
        {
            if (projectFileName.ContainsIgnoringCase("night") || projectFileName.ContainsIgnoringCase("sunset"))
            {
                return new NightTimeDashCamVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
            }
            else if (projectFileName.ContainsIgnoringCase("firework"))
            {
                return new FireworksDashCamVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
            }
            else if (projectFileName.ContainsIgnoringCase(VideoConstant.NissanAltima) || projectFileName.ContainsIgnoringCase(VideoConstant.GmcSierra))
            {
                return new CarRepairDashCamVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
            }
            else if (projectFileName.ContainsIgnoringCase("armchair"))
            {
                return new ArmchairDashCamVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
            }

            return new DayTimeDashCamVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }
        else if (projectFileName.ContainsIgnoringCase(FileExtension.HandymanTar.Value))
        {
            return new HandymanVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }
        else if (projectFileName.ContainsIgnoringCase(FileExtension.TechnologyTar.Value))
        {
            if (projectFileName.ContainsIgnoringCase("christmas light show"))
            {
                return new LightShowVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
            }

            return new TechTalkVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }
        else if (projectFileName.ContainsIgnoringCase(FileExtension.ToastmastersTar.Value))
        {
            return new ToastmastersVideoProject(projectFileName, _appSettings.BaseVideoDirectory);
        }

        throw new ArgumentException("Unable to determine video type", nameof(projectFileName));
    }


    public async Task CompressTarballsInArchiveFolderAsync(
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

}