using System.Collections.Generic;
using System.Threading.Tasks;
using Almostengr.VideoProcessor.Api.DataTransferObjects;
using Almostengr.VideoProcessor.Api.Enums;

namespace Almostengr.VideoProcessor.Api.Services.Data
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