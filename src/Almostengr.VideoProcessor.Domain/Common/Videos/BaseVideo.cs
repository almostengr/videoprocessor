using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public abstract record BaseVideo : BaseEntity
{
    public BaseVideo()
    {
        if (string.IsNullOrWhiteSpace(BaseDirectory))
        {
            throw new VideoInvalidBaseDirectoryException($"{nameof(BaseDirectory)} is invalid");
        }

        IncomingDirectory = Path.Combine(BaseDirectory, "incoming");
        UploadDirectory = Path.Combine(BaseDirectory, "upload");
        WorkingDirectory = Path.Combine(BaseDirectory, "working");
        ArchiveDirectory = Path.Combine(BaseDirectory, "archive");
        FfmpegInputFilePath = Path.Combine(WorkingDirectory, "ffmpeginput.txt");

        TarballFileName = string.Empty;
        TarballFilePath = string.Empty;
        Title = string.Empty;
        OutputFileName = string.Empty;
        OutputFilePath = string.Empty;
    }

    public string BaseDirectory { get; init; }
    public string TarballFilePath { get; private set; }
    public string TarballFileName { get; private set; }
    public string Title { get; private set; }
    public string OutputFileName { get; private set; }
    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string WorkingDirectory { get; }
    public string UploadDirectory { get; }
    public string OutputFilePath { get; private set; }
    public string FfmpegInputFilePath { get; }

    public abstract string ChannelBannerText();
    public abstract string TextColor();
    public abstract string BoxColor();

    internal void SetTarballFilePath(string? tarballFilePath)
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

        OutputFilePath = Path.Combine(UploadDirectory, OutputFileName);
    }
}
