using Almostengr.VideoProcessor.Core.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.Videos.DashCam;

public sealed record DashCamVideo : BaseVideo
{
    public DashCamVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
    }

    public override string[] BrandingTextOptions()
    {
        return new string[] { "Kenny Ram Dash Cam" };
    }

    internal override string SetTitle(string fileName)
    {
        if (fileName.ToLower().Contains("bad drivers of montgomery"))
        {
            throw new InvalidVideoTitleException();
        }

        return base.SetTitle(fileName);
    }
}