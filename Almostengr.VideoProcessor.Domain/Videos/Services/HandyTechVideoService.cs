using Almostengr.VideoProcessor.Application.Common;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Music.Services;
using Almostengr.VideoProcessor.Domain.Videos;

namespace Almostengr.VideoProcessor.Application.Video.Services;

public sealed class HandyTechVideoService : BaseVideoService, IHandyTechVideoService
{
    private readonly IFileSystemService _fileSystemService;
    private readonly IFfmpegService _ffmpegService;
    private readonly ITarballService _tarballService;
    private readonly IMusicService _musicService;
    private const int RHT_BORDER_WIDTH = 7;

    public HandyTechVideoService(IFileSystemService fileSystemService, IFfmpegService ffmpegService,
        ITarballService tarballService, IMusicService musicService) :
        base(fileSystemService, ffmpegService)
    {
        _fileSystemService = fileSystemService;
        _ffmpegService = ffmpegService;
        _tarballService = tarballService;
        _musicService = musicService;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            HandyTechVideo video = new HandyTechVideo();

            if (_fileSystemService.IsDiskSpaceAvailable(video.BaseDirectory) == false)
            {
                return Task.CompletedTask;
            }

            string? tarBallFilePath = _fileSystemService.GetRandomTarballFromDirectory(video.BaseDirectory);

            video.SetTarballFilePath(tarBallFilePath);

            _fileSystemService.DeleteDirectory(video.WorkingDirectory);
            _fileSystemService.CreateDirectory(video.WorkingDirectory);

            _tarballService.ExtractTarballContentsAsync(video.TarballFilePath, video.WorkingDirectory, stoppingToken);

            // lowercase all file names

            // add audio to timelapse videos

            // copy show intro to working directory

            // convert files to common format

            CreateFfmpegInputFile(video);

            string videoFilter = FfmpegVideoFilter(video);

            _ffmpegService.RenderVideoAsync(video.WorkingDirectory, videoFilter);

            _fileSystemService.MoveFile(video.TarballFilePath, video.ArchiveDirectory);

            _fileSystemService.DeleteDirectory(video.WorkingDirectory);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

    private void CreateFfmpegInputFile(HandyTechVideo video)
    {
        _fileSystemService.DeleteFile(video.FfmpegInputFilePath);

        using (StreamWriter writer = new StreamWriter(video.FfmpegInputFilePath))
        {
            var filesInDirectory = _fileSystemService.GetFilesInDirectory(video.WorkingDirectory)
                .Where(x => x.EndsWith(FileExtension.Mp4))
                .OrderBy(x => x)
                .ToArray();

            const string rhtservicesintro = "rhtservicesintro.1920x1080.mp4";
            const string file = "file";
            for (int i = 0; i < filesInDirectory.Length; i++)
            {
                if (filesInDirectory[i].Contains(rhtservicesintro))
                {
                    continue;
                }

                if (i == 1 && _fileSystemService.DoesFileExist(video.NoIntroFilePath()) == false)
                {
                    writer.WriteLine($"{file} '{rhtservicesintro}'");
                }

                writer.WriteLine($"{file} '{Path.GetFileName(filesInDirectory[i])}'");
            }
        }
    }

    private string FfmpegVideoFilter(HandyTechVideo handyTechVideo)
    {
        string videoFilter = $"drawtext=textfile:'{handyTechVideo.ChannelBannerText()}':";
        videoFilter += $"fontcolor={handyTechVideo.TextColor()}@{DIM_TEXT}:";
        videoFilter += $"fontsize={SMALL_FONT}:";
        videoFilter += $"{_upperRight}:";
        videoFilter += $"box=1:";
        videoFilter += $"boxborderw={RHT_BORDER_WIDTH.ToString()}:";
        videoFilter += $"boxcolor={handyTechVideo.BoxColor()}@{DIM_BACKGROUND}";
        return videoFilter;
    }

    private async Task AddAudioToTimelapseAsync(string workingDirectory, CancellationToken cancellationToken)
    {
        const string narration = "narration";
        const string narrative = "narrative";
        const string audio = "audio";

        var videoFiles = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(x => !x.Contains(narration) || !x.Contains(narrative))
            .Where(x => x.EndsWith(FileExtension.Mp4));

        var narrationFiles = _fileSystemService.GetFilesInDirectory(workingDirectory)
            .Where(x => x.Contains(narration) || x.Contains(narrative))
            .Where(x => x.EndsWith(FileExtension.Mp4) || x.EndsWith(FileExtension.Mkv))
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            var result = await _ffmpegService.FfprobeAsync(videoFileName, workingDirectory, cancellationToken);

            if (result.stdErr.ToLower().Contains(audio))
            {
                continue;
            }

            string? audioFile = narrationFiles.Where(
                    x => x.Contains(Path.GetFileNameWithoutExtension(videoFileName))
                )
                .SingleOrDefault();

            if (string.IsNullOrEmpty(audioFile))
            {
                audioFile = _musicService.GetRandomNonMixTrack();
            }

            string outputFileName = $"{Path.GetFileNameWithoutExtension(videoFileName)}.tmp{FileExtension.Mp4}";

            outputFileName = outputFileName.Replace(narration, string.Empty)
                .Replace(narrative, string.Empty);

            await _ffmpegService.FfmpegAsync(
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -i \"{audioFile}\" -vcodec h264_vaapi -shortest -map 0:v:0 -map 1:a:0 \"{outputFileName}\"",
                workingDirectory,
                cancellationToken);

            _fileSystemService.DeleteFile(videoFileName);
            _fileSystemService.MoveFile(Path.Combine(workingDirectory, outputFileName), videoFileName);
        }

        _fileSystemService.DeleteFiles(narrationFiles);
    }

    private async Task ConvertVideoFilesToCommonFormatAsync(string directory, HandyTechVideo handyTechVideo, CancellationToken cancellationToken)
    {
        var videoFiles = _fileSystemService.GetFilesInDirectory(directory)
            .Where(x => x.EndsWith(FileExtension.Mkv) || x.EndsWith(FileExtension.Mov) || x.EndsWith(FileExtension.Mp4))
            .OrderBy(x => x)
            .ToArray();

        foreach (var videoFileName in videoFiles)
        {
            var result = await _ffmpegService.FfprobeAsync(videoFileName, directory, cancellationToken);

            string scaledFile = $"{Path.GetFileNameWithoutExtension(videoFileName)}.{handyTechVideo.xResolution}x{handyTechVideo.yResolution}{FileExtension.Mp4}";

            if (result.stdErr.Contains($"{handyTechVideo.xResolution}x{handyTechVideo.yResolution}") &&
                result.stdErr.Contains($"{handyTechVideo.audioBitRate} Hz") &&
                result.stdErr.Contains($"196 kb/s") &&
                videoFileName.EndsWith(FileExtension.Mp4))
            {
                _fileSystemService.MoveFile(Path.Combine(directory, videoFileName), Path.Combine(directory, scaledFile));
                continue;
            }

            await _ffmpegService.FfmpegAsync(
                $"{LOG_ERRORS} {HW_OPTIONS} -i \"{Path.GetFileName(videoFileName)}\" -r 30 -vf \"scale_vaapi=w={handyTechVideo.xResolution}:h={handyTechVideo.yResolution}\" -vcodec h264_vaapi -ar {handyTechVideo.audioSampleRate} -b:a {handyTechVideo.audioBitRate} \"{scaledFile}\"",
                directory,
                cancellationToken);

            _fileSystemService.DeleteFile(Path.Combine(directory, videoFileName));

            string outputFileName = Path.GetFileNameWithoutExtension(videoFileName) + FileExtension.Mp4;
        }
    }

}