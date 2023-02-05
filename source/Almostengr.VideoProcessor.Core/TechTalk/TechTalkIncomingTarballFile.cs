using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk
{
    public sealed record TechTalkIncomingTarballFile : BaseIncomingTarballFile
    {
        public TechTalkIncomingTarballFile(string tarballFilePath) : base(tarballFilePath)
        {
        }
    }
}