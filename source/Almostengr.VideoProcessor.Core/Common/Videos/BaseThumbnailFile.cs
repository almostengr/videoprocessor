using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class BaseThumbnailFile
{
    internal string ThumbTxtFilePath { get; init; }

    public BaseThumbnailFile(string thumbTxtFilePath)
    {
        if (!thumbTxtFilePath.EndsWithIgnoringCase(FileExtension.ThumbTxt.Value))
        {
            throw new ArgumentException(Constant.FileTypeIsIncorrect, nameof(thumbTxtFilePath));
        }

        if (!File.Exists(thumbTxtFilePath))
        {
            throw new ArgumentException(Constant.FileDoesNotExist, nameof(thumbTxtFilePath));
        }

        ThumbTxtFilePath = thumbTxtFilePath;
    }

    public abstract string WebPageFileName();

    public string Title()
    {
        return Path.GetFileNameWithoutExtension(ThumbTxtFilePath);
    }

    public string ThumbnailFileName()
    {
        return Path.GetFileName(ThumbTxtFilePath)
            .ReplaceIgnoringCase(FileExtension.ThumbTxt.Value, FileExtension.Jpg.Value);
    }

    public string ThumbTxtFileName()
    {
        return Path.GetFileName(ThumbTxtFilePath);
    }
}
