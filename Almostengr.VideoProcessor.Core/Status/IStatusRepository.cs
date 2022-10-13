namespace Almostengr.VideoProcessor.Core.Status
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