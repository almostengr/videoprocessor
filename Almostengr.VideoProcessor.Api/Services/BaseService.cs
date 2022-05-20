using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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

            if (spaceRemaining > _appSettings.DiskSpaceThreshold)
            {
                return true;
            }

            _logger.LogError($"Disk space is too low. Disk space free: {(spaceRemaining * 100).ToString("###.##")}%");
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

        public void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
                _logger.LogInformation($"Removed file {fileName}");
            }
        }

        public void CreateDirectory(string pathName)
        {
            if (Directory.Exists(pathName) == false)
            {
                Directory.CreateDirectory(pathName);
                _logger.LogInformation($"Created directory {pathName}");
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

        public async Task ConfirmFileTransferCompleteAsync(string videoArchive)
        {
            _logger.LogInformation($"Confirming file transfer complete: {videoArchive}");

            FileInfo fileInfo = new FileInfo(videoArchive);
            long currentSize = fileInfo.Length;
            long previousSize = 0;

            while (currentSize != previousSize)
            {
                previousSize = currentSize;
                await Task.Delay(TimeSpan.FromSeconds(5));
                fileInfo.Refresh();
                currentSize = fileInfo.Length;
            }

            _logger.LogInformation($"Done confirming file transfer complete: {videoArchive}");
        }

    } // end of class BaseService
}
