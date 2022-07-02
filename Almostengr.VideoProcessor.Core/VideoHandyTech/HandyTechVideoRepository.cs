using Almostengr.VideoProcessor.Core.Database;
using Almostengr.VideoProcessor.Core.Status;

namespace Almostengr.VideoProcessor.Core.VideoHandyTech
{
    public sealed class HandyTechVideoRepository : IHandyTechVideoRepository
    {
        private readonly VideoDbContext _dbContext;

        public HandyTechVideoRepository(VideoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<StatusDto>> GetStatusAsync()
        {
            // return await _dbContext.Statuses
            //     .Where(s => s.Id == StatusKeys.DashStatus && s.Id == StatusKeys.DashFile)
            //     // .Select(s)
            throw new NotImplementedException();
        }

        public Task SaveChangesAsync()
        {
            throw new NotImplementedException();
        }

        public async Task UpsertStatusAsync(StatusDto statusDto)
        {
            throw new NotImplementedException();
        }
    }
}