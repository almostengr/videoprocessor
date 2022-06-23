using Almostengr.VideoProcessor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Almostengr.VideoProcessor.Api.Database
{
    public class VideoDbContext : DbContext, IVideoDbContext
    {
        public VideoDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Status> Statuses { get; set; }
    }
}