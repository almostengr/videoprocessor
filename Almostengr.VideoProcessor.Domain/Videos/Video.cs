using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Videos;

public sealed record Video : BaseEntity
{
    public Video(string tarballFilePath)
    {
        if (string.IsNullOrWhiteSpace(tarballFilePath))
        {
            throw new VideoTarballFilePathIsNullOrEmptyException($"{nameof(tarballFilePath)} is invalid");
        }

        if (tarballFilePath.EndsWith(FileExtension.Tar) == false && 
            tarballFilePath.EndsWith(FileExtension.TarXz) == false)
        {
            throw new VideoTarballFilePathHasWrongExtensionException();
        }

        FileInfo fileInfo = new FileInfo(tarballFilePath);
        if (fileInfo.Exists == false)
        {
            throw new VideoTarballFileDoesNotExistException($"{nameof(tarballFilePath)} does not exist");
        }

        TarballFilePath = tarballFilePath;

        TarballFileName = Path.GetFileName(TarballFilePath);

        Title = TarballFileName.Replace("/", string.Empty)
            .Replace(":", Constants.Whitespace)
            .Replace(FileExtension.Tar, Constants.Whitespace);

        OutputFileName = Title + FileExtension.Mp4;
    }

    public string TarballFilePath { get; }
    public string TarballFileName { get; }
    public string Title { get; }
    public string OutputFileName { get; }
}