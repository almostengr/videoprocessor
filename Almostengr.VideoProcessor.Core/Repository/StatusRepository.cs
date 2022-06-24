using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Core.Database;
using Almostengr.VideoProcessor.Core.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Enums;
using Almostengr.VideoProcessor.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Almostengr.VideoProcessor.Core.Repository
{
    public class StatusRepository : IStatusRepository
    {
        private readonly VideoDbContext _dbContext;

        public StatusRepository(VideoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<StatusDto>> GetAllAsync()
        {
            return await _dbContext.Statuses
                .Select(s => s.AsDto())
                .ToListAsync();
        }

        public async Task<StatusDto> GetByKeyAsync(StatusKeys key)
        {
            return await _dbContext.Statuses
                .Where(s => s.Id == (int) key)
                .Select(s => s.AsDto())
                .FirstOrDefaultAsync();
        }

        public async Task InsertAsync(StatusDto statusDto)
        {
            await _dbContext.Statuses.AddAsync(new Status(statusDto));
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        public async void Update(StatusDto statusDto)
        {
            var entity = await _dbContext.Statuses
                .Where(s => s.Id == (int) statusDto.Key)
                .FirstOrDefaultAsync();

            entity.Value = statusDto.Value;
            entity.LastChanged = DateTime.Now;

            _dbContext.Statuses.Update(entity);
        }
    }
}