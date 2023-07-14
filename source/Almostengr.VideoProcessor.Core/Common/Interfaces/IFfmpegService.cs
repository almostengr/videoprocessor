namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFfmpegService
{
    Task<(string stdOut, string stdErr)> FfprobeAsync(
        string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> AddAccAudioToVideoAsync(
        string videoFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithInputFileAndFiltersAsync(
        string ffmpegInputFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ConvertVideoFileToTsFormatAsync(
        string filePath, string outFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithFiltersAsync(
        string videoFilePath, string videoFilters, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> AdjustAudioVolumeAsync(
        string inputFilePath, string outputFilePath, float maxVolume, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> AnalyzeAudioVolumeAsync(
        string inputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithInputFileAndAudioAndFiltersAsync(
        string ffmpegInputFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ConvertImageFileToVideoAsync(
        string imageFile, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> CreateMusicMixTrackAsync(
        string ffmpegInputFile, string outputFile, string workingDirectory, CancellationToken cancellationToken);
}