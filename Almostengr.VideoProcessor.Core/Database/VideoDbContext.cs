using Almostengr.VideoProcessor.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Almostengr.VideoProcessor.Core.Database
{
    public class VideoDbContext : DbContext, IVideoDbContext
    {
        public VideoDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Status> Statuses { get; set; }
    }
}