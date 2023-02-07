using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseIncomingTarballFile
{
    protected BaseIncomingTarballFile(string tarballFilePath)
    {
        if (string.IsNullOrWhiteSpace(tarballFilePath) ||
            !tarballFilePath.ToLower().Contains(FileExtension.Tar.Value))
        {
            throw new TarballFilePathException("Tarball file path is invalid");
        }

        FilePath = tarballFilePath;
        FileName = Path.GetFileName(FilePath);

        VideoFileName = FileName.Replace(FileExtension.TarGz.Value, string.Empty)
            .Replace(FileExtension.TarXz.Value, string.Empty)
            .Replace(FileExtension.Tar.Value, string.Empty)
            + FileExtension.Mp4.Value;

        AudioFileName = VideoFileName.Replace(FileExtension.Mp4.Value, FileExtension.Mp3.Value);
        GraphicsFileName = VideoFileName.Replace(FileExtension.Mp4.Value, FileExtension.GraphicsAss.Value);
    }

    public string FilePath { get; init; }
    public string FileName { get; init; }
    public string VideoFileName { get; init; }
    public string AudioFileName { get; init; }
    public string GraphicsFileName { get; init; }
}