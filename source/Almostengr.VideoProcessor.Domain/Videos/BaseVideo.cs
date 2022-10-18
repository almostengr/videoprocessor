using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Videos;

public abstract record BaseVideo : BaseEntity
{
    internal readonly string RhtServicesIntroPath = "/mnt/d74511ce-4722-471d-8d27-05013fd521b3/ytvideostructure/rhtservicesintro.mp4";

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
        FfmpegInputFilePath = Path.Combine(WorkingDirectory, Constants.FfmpegInputFileName);

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

        SetOutputFileName(Title + FileExtension.Mp4);        
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
        Random random = new Random();
        string[] bannerText = {
            "rhtservices.net",
            "Robinson Handy and Technology Services",
            "rhtservices.net/facebook",
            "rhtservices.net/instagram",
            "rhtservices.net/youtube",
            "@rhtservicesllc"
            };

        return bannerText.ElementAt(random.Next(0, bannerText.Length));
    }

}
