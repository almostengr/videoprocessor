using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public sealed class VideoProjectArchiveFile
{
    public string FilePath { get; init; }

    public VideoProjectArchiveFile(string filePath)
    {
        if (
            !filePath.ToLower().EndsWith(FileExtension.Tar.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.TarXz.Value) &&
            !filePath.ToLower().EndsWith(FileExtension.TarGz.Value))
        {
            throw new ArgumentException("File path is not valid", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new ArgumentException("Project file does not exist", nameof(filePath));
        }

        FilePath = filePath;
    }
}