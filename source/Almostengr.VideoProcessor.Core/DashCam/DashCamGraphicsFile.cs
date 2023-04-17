using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.DashCam;

public sealed class DashCamGraphicsFile : AssSubtitleFile
{
    public DashCamGraphicsFile(string filePath) : base(filePath)
    {
    }

    public string VideoFileName()
    {
        return Path.GetFileNameWithoutExtension(FilePath) + FileExtension.Mp4.Value;
    }
}