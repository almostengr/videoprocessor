using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseIncomingTarballFile
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
            + FileExtension.Ts.Value;
    }

    public string FilePath { get; init; }
    public string FileName { get; init; }
    public string VideoFileName { get; init; }
}