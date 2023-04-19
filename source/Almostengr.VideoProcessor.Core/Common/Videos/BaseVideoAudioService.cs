using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Music.Services;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseVideoAudioService : BaseVideoService
{
    protected BaseVideoAudioService(AppSettings appSettings, IFfmpegService ffmpegService, IFileCompressionService gzipService, ITarballService tarballService, IFileSystemService fileSystemService, IRandomService randomService, IMusicService musicService, IAssSubtitleFileService assSubtitleFileService) : base(appSettings, ffmpegService, gzipService, tarballService, fileSystemService, randomService, musicService, assSubtitleFileService)
    {
    }

    public abstract Task ConvertVideoToMp3AudioAsync(CancellationToken cancellationToken);
}