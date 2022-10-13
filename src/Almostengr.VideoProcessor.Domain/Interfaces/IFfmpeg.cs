namespace Almostengr.VideoProcessor.Domain.Interfaces;

public interface IFfmpeg
{
    Task<(string stdOut, string stdErr)> FfprobeAsync(string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> FfmpegAsync(string arguments, string directory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> RenderVideoAsync(string ffmpegInputFile, string videoFilter, string outputFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> ImagesToVideoAsync(string imagePath, string outputFilePath, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdout, string stdErr)> AddAudioToVideoAsync(string videoFilePath, string audioFilePath, string outputFilePath, string workingDirectory, CancellationToken cancellationToken);
}