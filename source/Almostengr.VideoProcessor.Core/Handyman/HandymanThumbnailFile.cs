using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanThumbnailFile : BaseThumbnailFile
{
    public HandymanThumbnailFile(string thumbTxtFilePath) : base(thumbTxtFilePath)
    {
    }

    public override string WebPageFileName()
    {
        return "tnhandyman.html";
    }
}