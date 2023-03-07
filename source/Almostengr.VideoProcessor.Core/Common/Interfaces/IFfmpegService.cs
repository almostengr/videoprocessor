namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFfmpegService
{
    Task<(string stdOut, string stdErr)> FfprobeAsync(
        string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> FfmpegAsync(
        string arguments, string directory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsCopyAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertMp4VideoFileToTsFormatAsync(
        string videoFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> AddAccAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> RenderVideoWithFiltersAsync(
        string ffmpegInputFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConcatTsFilesToMp4FileAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithAudioAndFiltersAsync(
        string videoFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ConvertVideoFileToTsFormatAsync(string filePath, string outFilePath, CancellationToken cancellationToken);
}