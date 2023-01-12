using System.Text;
namespace Almostengr.VideoProcessor.Core.Videos;

public abstract class BaseVideoService : IBaseVideoService
{
    internal abstract string GetChannelBrandingText(string[] options);
    public abstract Task ProcessIncomingVideoTarballsAsync(CancellationToken cancellationToken);

    internal string FfmpegInputFileText(string[] filesInDirectory, string inputFilePath)
    {
        StringBuilder text = new();
        const string FILE = "file";
        foreach (var file in filesInDirectory)
        {
            text.Append($"{FILE} '{file}'");
        }

        return text.ToString();
    }
}