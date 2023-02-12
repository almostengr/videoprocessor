using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IThumbnailService
{
    void GenerateThumbnail(ThumbnailType type, string uploadDirectory, string thumbnailFileName, string title);
}