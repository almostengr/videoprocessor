namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IFfmpegService
{
    Task<(string stdOut, string stdErr)> FfprobeAsync(
        string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> ConvertVideoFileToMp3FileAsync(
        string videoInputFilePath, string audioOutputFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> AdjustAudioVolumeAsync(
        string inputFilePath, string outputFilePath, float maxVolume, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> AnalyzeAudioVolumeAsync(
        string inputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> CreateMusicMixTrackAsync(
        string ffmpegInputFile, string outputFile, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsync(
        string ffmpegInputFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithAudioAsync(
        string ffmpegInputFilePath, string audioFilePath, string outputFilePath, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoWithAudioAndFiltersAsync(
        string videoFilePath, string audioTrackFilePath, string videoFilter, string outputFilePath, CancellationToken cancellationToken);
}