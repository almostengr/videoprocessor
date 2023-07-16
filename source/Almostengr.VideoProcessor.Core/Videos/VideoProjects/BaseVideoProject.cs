using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;
using Almostengr.VideoProcessor.Core.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Videos;

public abstract partial class BaseVideoProject : IVideoProject
{
    public string _filePath { get; init; }
    public string BaseDirectory { get; init; }

    public BaseVideoProject(string filePath, string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (
            !filePath.EndsWithIgnoringCase(FileExtension.Tar.Value) &&
            !filePath.EndsWithIgnoringCase(FileExtension.TarGz.Value) &&
            !filePath.EndsWithIgnoringCase(FileExtension.TarXz.Value))
        {
            throw new ArgumentException(Constant.FileTypeIsIncorrect, nameof(filePath));
        }

        _filePath = filePath;

        BaseDirectory = baseDirectory;
    }

    public abstract string ArchiveDirectory();
    public abstract string UploadDirectory();
    public abstract string ArchiveFilePath();
    public abstract string UploadFilePath();
    public abstract List<string> BrandingTextOptions();

    public string FilePath()
    {
        return _filePath;
    }
    
    public virtual FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    public virtual string Title()
    {
        string title = FileName()
            .ReplaceIgnoringCase(FileExtension.Mp4.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.TarXz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.TarGz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty);

        const int MAX_TITLE_LENGTH = 100;

        if (title.Length > MAX_TITLE_LENGTH)
        {
            throw new VideoTitleTooLongException($"Title is {title.Length} characters long, greater than {MAX_TITLE_LENGTH}");
        }

        return title;
    }

    public string OutputFileName()
    {
        return Path.GetFileName(_filePath)
            .ReplaceIgnoringCase(FileExtension.TarXz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.TarGz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty)
            + FileExtension.Mp4.Value;
    }

    public string FileName()
    {
        return Path.GetFileName(_filePath);
    }

    public string ChannelBrandDrawTextFilter(string brandingText)
    {
        StringBuilder stringBuilder = new();
        stringBuilder.Append(
            new DrawTextFilter(brandingText,
            DrawTextFilterTextColor(),
            Opacity.Full,
            DrawTextFilterBackgroundColor(),
            Opacity.Medium,
            DrawTextPosition.ChannelBrand).ToString()
            );
        return stringBuilder.ToString();
    }
}
