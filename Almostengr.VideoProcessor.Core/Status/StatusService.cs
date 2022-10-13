namespace Almostengr.VideoProcessor.Core.Status
{
    public class StatusService : IStatusService
    {
        private readonly IStatusRepository _statusRepository;

        public StatusService(IStatusRepository statusRepository)
        {
            _statusRepository = statusRepository;
        }

        public async Task<StatusDto> GetByKeyAsync(StatusKeys key)
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
            var resource = await GetByKeyAsync(status.Key);
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