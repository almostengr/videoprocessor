using System.IO;
using System.Linq;
using Almostengr.VideoProcessor.Api.Configuration;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public abstract class BaseService : IBaseService
    {
        private readonly ILogger<BaseService> _logger;
        private readonly AppSettings _appSettings;

        public BaseService(ILogger<BaseService> logger, AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        public bool IsDiskSpaceAvailable(string directory)
        {
            double freeSpace = new DriveInfo(directory).AvailableFreeSpace;
            double totalSpace = new DriveInfo(directory).TotalSize;
            double spaceRemaining = (freeSpace / totalSpace);
            _logger.LogInformation($"Disk space remaining: {spaceRemaining.ToString()}");

            if (spaceRemaining > _appSettings.DiskSpaceThreshold)
            {
                return true;
            }

            _logger.LogError("Disk space is too low");
            return false;
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
            if (File.Exists(source))
            {
                _logger.LogInformation($"Moving file {source} to {destination}");
                CreateDirectory(destination);
                Directory.Move(source, destination);
            }
        }

        public string[] GetDirectoryContents(string path, string searchPattern = "*.*")
        {
            return Directory.GetFiles(path, searchPattern).OrderBy(x => x).ToArray();
        }
    } // end of class BaseService
}
