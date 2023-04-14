using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IThumbnailService
{
    void GenerateThumbnails<T>(string uploadDirectory, IEnumerable<T> thumbnailFiles) where T : BaseThumbnailFile;
}