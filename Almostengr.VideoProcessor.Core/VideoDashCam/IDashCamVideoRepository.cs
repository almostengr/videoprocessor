using Almostengr.VideoProcessor.Core.Status;

namespace Almostengr.VideoProcessor.Core.VideoDashCam
{
    public interface IDashCamVideoRepository
    {
        Task<IEnumerable<StatusDto>> GetStatusAsync();
        Task UpsertStatusAsync(StatusDto statusDto);
        Task SaveChangesAsync();
    }
}