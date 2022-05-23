using System.Collections.Generic;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Enums;
using Almostengr.VideoProcessor.Api.Models;

namespace Almostengr.VideoProcessor.Api.Repository
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