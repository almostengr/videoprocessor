using Almostengr.VideoProcessor.Core.Common.Videos;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Core.DashCam;

internal sealed record DashCamVideo : BaseVideo
{
    internal DashCamVideo(string baseDirectory, string archiveFileName) : base(baseDirectory, archiveFileName)
    {
    }

    internal override string[] BrandingTextOptions()
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