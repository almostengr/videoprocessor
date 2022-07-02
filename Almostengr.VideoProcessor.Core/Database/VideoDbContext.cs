using Almostengr.VideoProcessor.Core.Status;
using Microsoft.EntityFrameworkCore;

namespace Almostengr.VideoProcessor.Core.Database
{
    public class VideoDbContext : DbContext, IVideoDbContext
    {
        public VideoDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<StatusModel> Statuses { get; set; }
    }
}