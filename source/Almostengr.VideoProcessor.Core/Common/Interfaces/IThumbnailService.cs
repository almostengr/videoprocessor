using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IThumbnailService
{
    void GenerateThumbnail<T>(string uploadDirectory, T thumbnailFile) where T : BaseThumbnailFile;
}