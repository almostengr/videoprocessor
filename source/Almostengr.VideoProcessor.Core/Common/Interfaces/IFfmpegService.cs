namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFfmpegService
{
    Task<(string stdOut, string stdErr)> FfprobeAsync(
        string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> FfmpegAsync(
        string arguments, string directory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsCopyAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithMixTrackAsync(
        string ffmpegInputFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> AddAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> AddAccAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, string videoFilter, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertEndScreenImageToMp4VideoAsync(
        string endScreenImageFilePath, string endScreenAudioFilePath, string endScreenOutputFilePath, CancellationToken cancellationToken);
}