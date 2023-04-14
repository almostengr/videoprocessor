using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk;

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