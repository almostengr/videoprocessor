using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Toastmasters
{
    public sealed record ToastmastersIncomingTarballFile : BaseIncomingTarballFile
    {
        public ToastmastersIncomingTarballFile(string tarballFilePath) : base(tarballFilePath)
        {
        }
    }
}