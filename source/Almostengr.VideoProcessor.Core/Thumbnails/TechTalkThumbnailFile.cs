namespace Almostengr.VideoProcessor.Core.Thumbnails;

public sealed class TechTalkThumbnailFile : BaseThumbnailFile
{
    public TechTalkThumbnailFile(string thumbTxtFilePath) : base(thumbTxtFilePath)
    {
    }

    public override string WebPageFileName()
    {
        return "tntechtalk.html";
    }
}