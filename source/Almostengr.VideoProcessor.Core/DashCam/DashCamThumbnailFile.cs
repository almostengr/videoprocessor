using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamThumbnailFile : BaseThumbnailFile
{
    public DashCamThumbnailFile(string thumbTxtFilePath) : base(thumbTxtFilePath)
    {
    }

    public override string WebPageFileName()
    {
        return "tndashcam.html";
    }
}