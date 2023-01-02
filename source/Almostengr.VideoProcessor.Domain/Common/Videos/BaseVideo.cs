using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Common.Videos;

public abstract record BaseVideo : BaseEntity
{
    internal static Random _random = new Random();

    public BaseVideo(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new VideoInvalidBaseDirectoryException($"{nameof(BaseDirectory)} is invalid");
        }

        BaseDirectory = baseDirectory;

        IncomingDirectory = Path.Combine(BaseDirectory, DirectoryNames.Incoming);
        UploadDirectory = Path.Combine(BaseDirectory, DirectoryNames.Upload);
        WorkingDirectory = Path.Combine(BaseDirectory, DirectoryNames.Working);
        ArchiveDirectory = Path.Combine(BaseDirectory, DirectoryNames.Archive);
        FfmpegInputFilePath = Path.Combine(WorkingDirectory, Constant.FfmpegInputFileName);

        TarballFileName = string.Empty;
        TarballFilePath = string.Empty;
        TarballArchiveFilePath = string.Empty;
        Title = string.Empty;
        OutputFileName = string.Empty;
        OutputFilePath = string.Empty;
        SubtitleFilePath = string.Empty;
    }

    public string BaseDirectory { get; init; }
    public string TarballFilePath { get; private set; }
    public string TarballFileName { get; private set; }
    public string Title { get; private set; }
    public string OutputFileName { get; private set; }
    public string IncomingDirectory { get; }
    public string ArchiveDirectory { get; }
    public string TarballArchiveFilePath { get; private set; }
    public string WorkingDirectory { get; }
    public string UploadDirectory { get; }
    public string OutputFilePath { get; private set; }
    public string FfmpegInputFilePath { get; }
    public string SubtitleFilePath { get; private set; }

    public abstract string ChannelBannerText();
    public abstract string BannerTextColor();
    public abstract string BannerBackgroundColor();
    public abstract string SubtitleTextColor();
    public abstract string SubtitleBackgroundColor();

    public void SetSubtitleFilePath(string? subtitleFilePath)
    {
        if (string.IsNullOrWhiteSpace(subtitleFilePath))
        {
            // throw new SubtitleFilePathIsNullOrWhiteSpaceException();
            return;
        }

        SubtitleFilePath = subtitleFilePath;
    }

    public void SetTarballFilePath(string? tarballFilePath)
    {
        if (string.IsNullOrWhiteSpace(tarballFilePath))
        {
            throw new VideoTarballFilePathIsNullOrEmptyException($"{nameof(tarballFilePath)} is invalid");
        }

        if (tarballFilePath.EndsWith(FileExtension.Tar) == false &&
            tarballFilePath.EndsWith(FileExtension.TarXz) == false &&
            tarballFilePath.EndsWith(FileExtension.TarGz) == false)
        {
            throw new VideoTarballFilePathHasWrongExtensionException();
        }

        TarballFilePath = tarballFilePath;
        TarballFileName = Path.GetFileName(TarballFilePath);

        Title = TarballFileName.Replace("/", string.Empty)
            .Replace(":", string.Empty)
            .Replace(FileExtension.TarGz, string.Empty)
            .Replace(FileExtension.TarXz, string.Empty)
            .Replace(FileExtension.Tar, string.Empty);

        SetOutputFileName(
            TarballFileName.Replace("/", string.Empty)
            .Replace(":", string.Empty)
            .Replace(FileExtension.TarGz, string.Empty)
            .Replace(FileExtension.TarXz, string.Empty)
            .Replace(FileExtension.Tar, string.Empty)
                + FileExtension.Mp4);

        TarballArchiveFilePath = Path.Combine(ArchiveDirectory, TarballFileName);
    }

    internal virtual void SetOutputFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new VideoOutputFileNameIsNullOrWhiteSpace();
        }

        OutputFileName = fileName;
        OutputFilePath = Path.Combine(UploadDirectory, OutputFileName);
    }

    protected string ChannelBannerTextRhtServices()
    {
        string[] bannerText = {
            "rhtservices.net",
            "Robinson Handy and Technology Services",
            "rhtservices.net/courses",
            "rhtservices.net/facebook",
            "rhtservices.net/instagram",
            // "rhtservices.net/youtube",
            "@rhtservicesllc"
            };

        return bannerText.ElementAt(_random.Next(0, bannerText.Length));
    }

}
