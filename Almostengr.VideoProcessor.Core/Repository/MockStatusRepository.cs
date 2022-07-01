using Almostengr.VideoProcessor.Core.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Enums;

namespace Almostengr.VideoProcessor.Core.Repository
{
    public class MockStatusRepository : IStatusRepository
    {
        public async Task<List<StatusDto>> GetAllAsync()
        {
            return await Task.Run(() => new List<StatusDto>());
        }

        public async Task<StatusDto> GetByKeyAsync(StatusKeys key)
        {
            return await Task.Run(() => new StatusDto());
        }

        public async Task InsertAsync(StatusDto entity)
        {
            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await Task.CompletedTask;
        }

        public void Update(StatusDto entity)
        {
            return;
        }
    }
}