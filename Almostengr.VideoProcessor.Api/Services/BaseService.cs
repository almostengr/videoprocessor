using System.IO;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public abstract class BaseService : IBaseService
    {
        private readonly ILogger<BaseService> _logger;

        public BaseService(ILogger<BaseService> logger)
        {
            _logger = logger;
        }

        public bool IsDiskSpaceAvailable(string directory)
        {
            var freeSpace = new DriveInfo(directory).AvailableFreeSpace;
            var totalSpace = new DriveInfo(directory).TotalSize;
            return (freeSpace / totalSpace) > 0.05;
        }

        public void RemoveFile(string filename)
        {
            Directory.Delete(filename, true);
            _logger.LogInformation($"Removed file {filename}");
        }

        public void CreateDirectoryIfNotExists(string directory)
        {
            if (Directory.Exists(directory) == false)
            {
                Directory.CreateDirectory(directory);
                _logger.LogInformation($"Created directory {directory}");
            }
        }
    }
}