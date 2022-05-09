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
            double freeSpace = new DriveInfo(directory).AvailableFreeSpace;
            double totalSpace = new DriveInfo(directory).TotalSize;
            double spaceRemaining = (freeSpace / totalSpace);
            _logger.LogInformation($"Disk space remaining: {spaceRemaining.ToString()}");
            return spaceRemaining > 0.05;
        }

        public void DeleteDirectory(string directoryName)
        {
            if (Directory.Exists(directoryName))
            {
                Directory.Delete(directoryName, true);
                _logger.LogInformation($"Removed directory {directoryName}");
            }
        }

        public void DeleteFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
                _logger.LogInformation($"Removed file {filename}");
            }
        }

        public void CreateDirectory(string pathname)
        {
            if (Directory.Exists(pathname) == false)
            {
                Directory.CreateDirectory(pathname);
                _logger.LogInformation($"Created directory {pathname}");
            }
        }

        public void MoveFile(string source, string destination)
        {
            if (File.Exists(source) && Directory.Exists(destination))
            {
                _logger.LogInformation($"Moving file {source} to {destination}");
                Directory.Move(source, destination);
            }
        }

        public string[] GetDirectoryContents(string path, string searchPattern = "*.*")
        {
            return Directory.GetFiles(path, searchPattern);
        }
    } // end of class BaseService
}
