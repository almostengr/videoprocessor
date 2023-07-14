namespace Almostengr.VideoProcessor.Core.Thumbnails;

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