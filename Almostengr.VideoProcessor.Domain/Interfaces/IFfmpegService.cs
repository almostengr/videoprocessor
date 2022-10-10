namespace Almostengr.VideoProcessor.Application.Common;

public interface IFfmpegService
{
    Task<(string stdOut, string stdErr)> RenderVideoAsync(string arguments, string workingDirectory);
    Task<(string stdOut, string stdErr)> FfprobeAsync(string videoFileName, string workingDirectory, CancellationToken cancellationToken);
    Task<(string stdOut, string stdErr)> FfmpegAsync(string arguments, string directory, CancellationToken cancellationToken);

}