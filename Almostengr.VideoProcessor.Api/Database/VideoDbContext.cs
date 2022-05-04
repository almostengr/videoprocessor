using Almostengr.VideoProcessor.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Almostengr.VideoProcessor.Api.Database
{
    public class VideoDbContext : DbContext
    {
        public VideoDbContext(DbContextOptions<VideoDbContext> options) : base(options)
        {
        }

        public DbSet<Status> Status { get; set; }
    }
}