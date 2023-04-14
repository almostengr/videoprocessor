using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

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
            throw new ArgumentException(Constant.FileTypeIsIncorrect, nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new ArgumentException(Constant.FileDoesNotExist, nameof(filePath));
        }

        FilePath = filePath;
    }
}