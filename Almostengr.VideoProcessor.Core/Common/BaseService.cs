using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Core.Common
{
    public abstract class BaseService
    {
        private readonly ILogger<BaseService> _logger;

        public BaseService(ILogger<BaseService> logger)
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

        public bool DoesFileExist(string directory, string fileName)
        {
            return DoesFileExist(Path.Combine(directory, fileName));
        }

        public void CopyFile(string sourceFile, string destinationFile)
        {
            _logger.LogInformation($"Copying {sourceFile} to {destinationFile}");
            File.Copy(sourceFile, destinationFile);
        }

        public async Task<(string stdOut, string stdErr)> RunCommandAsync(
            string program, string arguments, string workingDirectory, CancellationToken cancellationToken, int alarmTime = 30)
        {
            _logger.LogInformation($"{program} {arguments}");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = program,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            await process.WaitForExitAsync();

            _logger.LogInformation($"Exit code: {process.ExitCode}");

            process.Close();

            int errorCount = error.Split("\n")
                .Where(x =>
                    !x.Contains("libva: /usr/lib/x86_64-linux-gnu/dri/iHD_drv_video.so init failed") &&
                    !x.Contains("Output file is empty, nothing was encoded (check -ss / -t / -frames parameters if used") &&
                    !x.Contains("deprecated pixel format used, make sure you did set range correctly") &&
                    !x.Equals("")
                )
                .ToArray()
                .Count();

            if (errorCount > 0 && program == ProgramPaths.FfprobeBinary == false)
            {
                _logger.LogError(error);
                throw new ArgumentException("Errors occurred when running the command");
            }

            return await Task.FromResult((output, error));
        }
    }
}
