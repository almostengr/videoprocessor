using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed record HandymanIncomingTarballFile : BaseIncomingTarballFile
{
    public HandymanIncomingTarballFile(string tarballFilePath) : base(tarballFilePath)
    {
    }
}