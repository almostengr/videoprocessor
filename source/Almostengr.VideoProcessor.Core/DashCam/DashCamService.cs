using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Music.Services;
using Almostengr.VideoProcessor.Core.Common.Videos;
using System.Text;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamService : BaseVideoService, IDashCamVideoService
{
    private readonly ILoggerService<DashCamService> _loggerService;
    private readonly ICsvGraphicsFileService _csvGraphicsFileService;
    private readonly TimeSpan _subtitleDuration = TimeSpan.FromSeconds(5);

    public DashCamService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService,
        ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService,
        ILoggerService<DashCamService> loggerService, IMusicService musicService,
        ICsvGraphicsFileService csvGraphicsFileService,
        IAssSubtitleFileService assSubtitleFileService,
        IXzFileCompressionService xzFileService, IGzFileCompressionService gzFileService) :
        base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
        IncomingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Incoming);
        WorkingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Working);
        ArchiveDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Archive);
        UploadingDirectory = Path.Combine(_appSettings.DashCamDirectory, DirectoryName.Uploading);
        _loggerService = loggerService;
        _csvGraphicsFileService = csvGraphicsFileService;
    }

    public override async Task CompressTarballsInArchiveFolderAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CompressTarballsInArchiveFolderAsync(ArchiveDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task CreateTarballsFromDirectoriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await base.CreateTarballsFromDirectoriesAsync(IncomingDirectory, cancellationToken);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
        }
    }

    public override async Task ProcessVideoProjectAsync(CancellationToken cancellationToken)
    {
        string? readyFile = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.Ready.Value))
            .FirstOrDefault();

        if (readyFile == null)
        {
            return;
        }

        string projectFileName =
            Path.GetFileName(readyFile.ReplaceIgnoringCase(FileExtension.Ready.Value, FileExtension.Tar.Value));
        DashCamVideoProject? project = _fileSystemService.GetFilesInDirectory(IncomingDirectory)
           .Where(f => f.ContainsIgnoringCase(projectFileName))
           .Select(f => new DashCamVideoProject(f))
           .SingleOrDefault();

        if (project == null)
        {
            return;
        }

        try
        {
            _fileSystemService.DeleteDirectory(WorkingDirectory);
            _fileSystemService.CreateDirectory(WorkingDirectory);

            await _tarballService.ExtractTarballContentsAsync(project.FilePath, WorkingDirectory, cancellationToken);

            StopProcessingIfKdenliveFileExists(WorkingDirectory);
            StopProcessingIfDetailsTxtFileExists(WorkingDirectory);
            StopProcessingIfFfmpegInputTxtFileExists(WorkingDirectory);

            _fileSystemService.PrepareAllFilesInDirectory(WorkingDirectory);

            var textOptions = project.BrandingTextOptions().ToList();
            string brandingText = textOptions[_randomService.Next(0, textOptions.Count)];

            StringBuilder videoFilters = new(project.ChannelBrandDrawTextFilter(brandingText));

            var videoClips = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                .Where(f => f.FullName.IsVideoFile())
                .OrderBy(f => f.Name);

            int counter = -1;
            StringBuilder videoChapters = new();
            foreach (var clip in videoClips)
            {
                counter++;

                string outFilePath = clip.FullName + FileExtension.Ts.Value;

                await _ffmpegService.ConvertVideoFileToTsFormatAsync(clip.FullName, outFilePath, cancellationToken);

                string? streetsFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                    .Where(f => f.EndsWithIgnoringCase(Path.GetFileNameWithoutExtension(clip.FullName) + FileExtension.StreetsCsv.Value))
                    .SingleOrDefault();

                if (streetsFile == null ||
                    project.SubType == DashCamVideoProject.DashCamVideoType.Fireworks ||
                    project.SubType == DashCamVideoProject.DashCamVideoType.CarRepair)
                {
                    continue;
                }

                var graphicsContent = _csvGraphicsFileService.ReadFile(streetsFile);

                foreach (var graphic in graphicsContent)
                {
                    videoFilters.Append(Constant.CommaSpace);

                    FfMpegColor textColor = FfMpegColor.White;
                    FfMpegColor bgColor = FfMpegColor.Green;
                    Opacity bgOpacity = Opacity.Full;
                    const string INFO = "info";

                    if (
                        graphic.Text.ContainsIgnoringCase(INFO) ||
                        graphic.Text.ContainsIgnoringCase("mile drive")
                        )
                    {
                        graphic.SetText(graphic.Text.ReplaceIgnoringCase(INFO, string.Empty));
                        bgColor = FfMpegColor.Black;
                        bgOpacity = Opacity.Medium;
                    }
                    else if (graphic.Text.ContainsIgnoringCase("speed limit"))
                    {
                        bgColor = FfMpegColor.White;
                        textColor = FfMpegColor.Black;
                    }

                    var startTime = graphic.StartTime.Add(new TimeSpan(0, counter * 2, 0));
                    var endTime = startTime.Add(_subtitleDuration);
                    videoFilters.Append(
                        new DrawTextFilter(
                            graphic.Text, textColor, Opacity.Full, bgColor, bgOpacity, DrawTextPosition.SubtitlePrimary, startTime, endTime).ToString());

                    videoChapters.Append($"{startTime.ToString()} {graphic.Text}" + Environment.NewLine);
                }
            }

            // string chaptersFilePath = Path.Combine(UploadingDirectory, project.ChaptersFileName());
            // _fileSystemService.SaveFileContents(chaptersFilePath, videoChapters.ToString());

            string? ffmpegInputFilePath = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(FileExtension.FfmpegInput.Value))
                .SingleOrDefault();

            if (string.IsNullOrEmpty(ffmpegInputFilePath))
            {
                var tsVideoFiles = _fileSystemService.GetFilesInDirectoryWithFileInfo(WorkingDirectory)
                    .Where(f => f.Name.EndsWithIgnoringCase(FileExtension.Ts.Value))
                    .OrderBy(f => f.Name)
                    .Select(f => f.FullName);

                ffmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);
                CreateFfmpegInputFile(tsVideoFiles, ffmpegInputFilePath);
            }

            int lowestTimeStamp = 21;
            int maxTimeStamp = lowestTimeStamp + 240;
            for (var i = 0; i < videoClips.Count(); i++)
            {
                int randomTime = _randomService.Next(lowestTimeStamp, maxTimeStamp);
                lowestTimeStamp = randomTime;

                if (i % 2 == 0)
                {
                    continue;
                }

                TimeSpan startTime = TimeSpan.FromSeconds(randomTime);
                TimeSpan endTime = startTime.Add(_subtitleDuration);
                videoFilters.Append(Constant.CommaSpace);
                videoFilters.Append(new DrawTextFilter("Please like and subscribe!", FfMpegColor.White, Opacity.Full, FfMpegColor.Red, Opacity.Full, DrawTextPosition.LowerRight, startTime, endTime).ToString());
            }

            string outputFilePath = Path.Combine(WorkingDirectory, project.VideoFileName());
            var audioFile = _musicService.GetRandomMixTrack();

            if (project.SubType == DashCamVideoProject.DashCamVideoType.CarRepair)
            {
                await _ffmpegService.RenderVideoWithFiltersAsync(
                    ffmpegInputFilePath, videoFilters.ToString(), outputFilePath, cancellationToken);
            }
            else
            {
                await _ffmpegService.RenderVideoWithInputFileAndAudioAndFiltersAsync(
                    ffmpegInputFilePath, audioFile.FilePath, videoFilters.ToString(), outputFilePath, cancellationToken);
            }

            _fileSystemService.MoveFile(project.FilePath, Path.Combine(ArchiveDirectory, project.FileName()));
            _fileSystemService.MoveFile(outputFilePath, Path.Combine(UploadingDirectory, project.VideoFileName()));

            const string DESCRIPTION_TXT = "description.txt";
            var descriptionFile = _fileSystemService.GetFilesInDirectory(WorkingDirectory)
                .Where(f => f.EndsWithIgnoringCase(DESCRIPTION_TXT))
                .SingleOrDefault();

            string uploadDescriptionTxt = Path.Combine(UploadingDirectory, project.FileName() + DESCRIPTION_TXT);
            if (descriptionFile != null)
            {
                _fileSystemService.MoveFile(descriptionFile, uploadDescriptionTxt);
            }

            _fileSystemService.DeleteFile(readyFile);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }
        catch (Exception ex)
        {
            _loggerService.LogError(ex, ex.Message);
            _loggerService.LogErrorProcessingFile(project.FilePath, ex);
            _fileSystemService.MoveFile(readyFile, readyFile + FileExtension.Err.Value);
            _fileSystemService.DeleteDirectory(WorkingDirectory);
        }

    }
}