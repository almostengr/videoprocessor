using Almostengr.VideoProcessor.Core.Videos;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFfmpegService
{
    Task<(string stdOut, string stdErr)> FfprobeAsync(string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> FfmpegAsync(string arguments, string directory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsCopyAsync(string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsync(string ffmpegInputFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithMixTrackAsync(string ffmpegInputFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(string videoFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> AddAudioToVideoAsync(string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ImagesToVideoAsync(string imageFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> CreateThumbnailsFromVideoFilesAsync<T>(T video, CancellationToken cancellationToken) where T : BaseVideo;
    Task<(string stdout, string stdErr)> CreateTitleTextVideoAsync<T>(T video, CancellationToken cancellationToken) where T : BaseVideo;
    Task<(string stdout, string stdErr)> ConvertVideoToMp3AudioAsync(string file, string outputFilePath, string workingDirectory, CancellationToken cancellationToken);
}