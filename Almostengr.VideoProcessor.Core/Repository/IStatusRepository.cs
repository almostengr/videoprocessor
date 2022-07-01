using Almostengr.VideoProcessor.Core.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Enums;

namespace Almostengr.VideoProcessor.Core.Repository
{
    public interface IStatusRepository
    {
        Task<StatusDto> GetByKeyAsync(StatusKeys key);
        Task<List<StatusDto>> GetAllAsync();
        Task InsertAsync(StatusDto entity);
        void Update(StatusDto entity);
        Task SaveChangesAsync();
    }
}