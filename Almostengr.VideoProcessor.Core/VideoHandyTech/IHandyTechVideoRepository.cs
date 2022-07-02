using Almostengr.VideoProcessor.Core.Status;

namespace Almostengr.VideoProcessor.Core.VideoHandyTech
{
    public interface IHandyTechVideoRepository
    {
        Task<IEnumerable<StatusDto>> GetStatusAsync();
        Task UpsertStatusAsync(StatusDto statusDto);
        Task SaveChangesAsync();
    }
}