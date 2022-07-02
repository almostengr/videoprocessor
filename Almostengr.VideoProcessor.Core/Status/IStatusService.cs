namespace Almostengr.VideoProcessor.Core.Status
{
    public interface IStatusService
    {
        Task<StatusDto> GetByKeyAsync(StatusKeys key);
        Task<List<StatusDto>> GetListAsync();
        Task InsertAsync(StatusDto status);
        Task SaveChangesAsync();
        Task UpsertAsync(StatusDto status);
        Task UpsertAsync(StatusKeys key, string value);
    }
}