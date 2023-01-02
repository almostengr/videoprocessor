using System.Text;
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
        VideoFilter = string.Empty;

        AddChannelBannerTextFilter();
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
    public string VideoFilter { get; private set; }

    public abstract string ChannelBannerText();
    public abstract string BannerTextColor();
    public abstract string BannerBackgroundColor();
    public abstract string SubtitleTextColor();
    public abstract string SubtitleBackgroundColor();

    public virtual void AddChannelBannerTextFilter()
    {
        if (VideoFilter.Contains(ChannelBannerText()))
        {
            return;
        }

        StringBuilder textFilter = new($"drawtext=textfile:'{ChannelBannerText()}':");
        textFilter.Append($"fontcolor={BannerTextColor()}:");
        textFilter.Append($"fontsize={FfmpegFontSize.Medium}:");
        textFilter.Append($"{DrawTextPosition.UpperRight}:");
        textFilter.Append(Constant.BorderChannelText);
        textFilter.Append($"boxcolor={BannerBackgroundColor()}@{Constant.DimBackground}");
        VideoFilter += textFilter.ToString();
    }

    private string FilterDuration()
    {
        return $"enable=lt(mod(t\\,120)\\,{Constant.CALL_TO_ACTION_DURATION_SECONDS})";
    }

    public string GetSubscribeTextFilter()
    {
        return $"drawtext=text:'SUBSCRIBE for future videos':fontcolor={FfMpegColors.White}:fontsize={FfmpegFontSize.Medium}:{DrawTextPosition.LowerLeft}:boxcolor={FfMpegColors.Red}:box=1:boxborderw=10:{FilterDuration()}";
    }

    public void AddSubscribeTextFilter()
    {
        VideoFilter += Constant.Comma;
        VideoFilter += GetSubscribeTextFilter();
    }

    public string GetLikeTextFilter()
    {
        return $"drawtext=text:'GIVE US A THUMBS UP!':fontcolor={FfMpegColors.White}:fontsize={FfmpegFontSize.Medium}:{DrawTextPosition.LowerLeft}:boxcolor={FfMpegColors.Blue}:box=1:boxborderw=10:{FilterDuration()}";
    }

    public virtual void AddDrawTextFilter(
        string text, string textColor, string textBrightness, string fontSize, string position,
        string backgroundColor, string backgroundBrightness, string duration = "")
    {
        StringBuilder textFilter = new();
        textFilter.Append(Constant.CommaSpace);
        textFilter.Append($"drawtext=textfile:'{text}':");
        textFilter.Append($"fontcolor={textColor}@{textBrightness}:");
        textFilter.Append($"fontsize={fontSize}:");
        textFilter.Append($"{position}:");
        textFilter.Append(Constant.BorderChannelText);
        textFilter.Append($"boxcolor={backgroundColor}@{backgroundBrightness}");

        if (duration.Length > 0)
        {
            textFilter.Append(":");
            textFilter.Append(duration);
        }

        VideoFilter += textFilter.ToString();
    }

    public virtual void AddDrawTextFilter(string filter)
    {
        StringBuilder textFilter = new();
        textFilter.Append(Constant.CommaSpace);
        textFilter.Append(filter);
        VideoFilter += textFilter.ToString();
    }

    public void SetSubtitleFilePath(string? subtitleFilePath)
    {
        if (string.IsNullOrWhiteSpace(subtitleFilePath))
        {
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