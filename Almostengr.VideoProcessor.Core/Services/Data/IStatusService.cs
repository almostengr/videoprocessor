using Almostengr.VideoProcessor.Core.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Enums;

namespace Almostengr.VideoProcessor.Core.Services.Data
{
    public interface IStatusService
    {
        Task<StatusDto> GetByIdAsync(StatusKeys key);
        Task<List<StatusDto>> GetListAsync();
        Task InsertAsync(StatusDto status);
        Task SaveChangesAsync();
        Task UpsertAsync(StatusDto status);
        Task UpsertAsync(StatusKeys key, string value);
    }
}