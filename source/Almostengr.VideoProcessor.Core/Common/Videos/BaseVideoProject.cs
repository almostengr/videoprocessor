using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract partial class BaseVideoProject
{
    public string FilePath { get; init; }

    public BaseVideoProject(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(Constant.ArgumentCannotBeNullOrEmpty, nameof(filePath));
        }

        if (
            !filePath.EndsWithIgnoringCase(FileExtension.Tar.Value) &&
            !filePath.EndsWithIgnoringCase(FileExtension.TarGz.Value) &&
            !filePath.EndsWithIgnoringCase(FileExtension.TarXz.Value))
        {
            throw new ArgumentException(Constant.FileTypeIsIncorrect, nameof(filePath));
        }

        FilePath = filePath;
    }

    public abstract IEnumerable<string> BrandingTextOptions();

    public virtual FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Black;
    }

    public virtual FfMpegColor DrawTextFilterTextColor()
    {
        return FfMpegColor.White;
    }

    public string ThumbnailFileName()
    {
        return Title() + FileExtension.ThumbTxt.Value;
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

    public string VideoFileName()
    {
        return Path.GetFileName(FilePath)
            .ReplaceIgnoringCase(FileExtension.TarXz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.TarGz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty)
            + FileExtension.Mp4.Value;
    }

    public string ChaptersFileName()
    {
        return Path.GetFileName(FilePath)
            .ReplaceIgnoringCase(FileExtension.TarXz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.TarGz.Value, string.Empty)
            .ReplaceIgnoringCase(FileExtension.Tar.Value, string.Empty)
            .Replace(Constant.Colon, string.Empty)
            + ".chapters.txt";
    }

    public string FileName()
    {
        return Path.GetFileName(FilePath);
    }

    internal string ChannelBrandDrawTextFilter(string brandingText)
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
