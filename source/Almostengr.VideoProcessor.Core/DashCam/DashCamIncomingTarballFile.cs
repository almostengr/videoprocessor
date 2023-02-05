using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed record DashCamIncomingTarballFile : BaseIncomingTarballFile
{
    public DashCamIncomingTarballFile(string tarballFilePath) : base(tarballFilePath)
    {
    }
}