using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.Services.FileSystem
{
    public class FileSystemService : IFileSystemService
    {
        private readonly ILogger<FileSystemService> _logger;

        public FileSystemService(ILogger<FileSystemService> logger)
        {
            _logger = logger;
        }

        public bool IsDiskSpaceAvailable(string directory, double threshold)
        {
            double freeSpace = new DriveInfo(directory).AvailableFreeSpace;
            double totalSpace = new DriveInfo(directory).TotalSize;
            double spaceRemaining = (freeSpace / totalSpace);

            if (spaceRemaining > threshold)
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
                _logger.LogInformation($"Removing directory {directoryName}");
                Directory.Delete(directoryName, true);
            }
        }

        public void DeleteDirectories(string[] directoryNames)
        {
            foreach(var directory in directoryNames)
            {
                DeleteDirectory(directory);
            }
        }

        public void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                _logger.LogInformation($"Removing file {fileName}");
                File.Delete(fileName);
            }
        }

        public void DeleteFiles(string[] fileNames)
        {
            foreach(var file in fileNames)
            {
                DeleteFile(file);
            }
        }

        public void CreateDirectory(string pathName)
        {
            if (Directory.Exists(pathName) == false)
            {
                _logger.LogInformation($"Creating directory {pathName}");
                Directory.CreateDirectory(pathName);
            }
        }

        public void MoveFile(string source, string destination)
        {
            if (File.Exists(source))
            {
                _logger.LogInformation($"Moving file {source} to {destination}");
                Directory.Move(source, destination);
            }
        }

        public string[] GetDirectoriesInDirectory(string path)
        {
            return Directory.GetDirectories(path)
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetFilesInDirectory(string path, string searchPattern = "*.*")
        {
            return Directory.GetFiles(path, searchPattern)
                .OrderBy(x => x)
                .ToArray();
        }

        public string[] GetFilesInDirectory(string path)
        {
            return Directory.GetFiles(path)
                .OrderBy(x => x)
                .ToArray();
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
        }

        public bool DoesFileExist(string filePath)
        {
            return File.Exists(filePath) ? true : false;
        }

        public void CopyFile(string sourceFile, string destinationFile)
        {
            _logger.LogInformation($"Copying {sourceFile} to {destinationFile}");
            File.Copy(sourceFile, destinationFile);
        }
    }
}
