using System.Collections.Generic;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Core.DataTransferObjects;
using Almostengr.VideoProcessor.Core.Enums;
using Almostengr.VideoProcessor.Core.Repository;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.Services.Data
{
    public class StatusService : IStatusService
    {
        private readonly IStatusRepository _statusRepository;
        private readonly ILogger<StatusService> _logger;

        public StatusService(IStatusRepository statusRepository, ILogger<StatusService> logger)
        {
            _statusRepository = statusRepository;
            _logger = logger;
        }

        public async Task<StatusDto> GetByIdAsync(StatusKeys key)
        {
            return await _statusRepository.GetByKeyAsync(key);
        }

        public async Task<List<StatusDto>> GetListAsync()
        {
            return await _statusRepository.GetAllAsync();
        }

        public async Task InsertAsync(StatusDto status)
        {
            await _statusRepository.InsertAsync(status);
        }

        public async Task UpsertAsync(StatusDto status)
        {
            var resource = await GetByIdAsync(status.Key);
            if (resource == null)
            {
                await InsertAsync(status);
                return;
            }

            _statusRepository.Update(status);
        }

        public async Task UpsertAsync(StatusKeys key, string value)
        {
            StatusDto dto = new StatusDto{
                Key = key,
                Value = value,
            };

            await UpsertAsync(dto);
        }

        public async Task SaveChangesAsync()
        {
            await _statusRepository.SaveChangesAsync();
        }
    }
}