using Almostengr.VideoProcessor.Application.Common;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class FileSystemService : IFileSystemService
{
    private readonly Random _random;

    public FileSystemService()
    {
        _random = new Random();
    }

    public void CreateDirectory(string directory)
    {
        Directory.CreateDirectory(directory);
    }

    public void CopyFile(string sourceFile, string destinationDirectory)
    {
        File.Copy(sourceFile, destinationDirectory);
    }

    public void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public void DeleteDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, true);
        }
    }

    public string? GetRandomTarballFromDirectory(string directory)
    {
        return GetFilesInDirectory(directory)
            .Where(f => f.Contains(FileExtension.Tar))
            .Where(f => f.StartsWith(".") == false)
            .OrderBy(f => _random.Next()).Take(1)
            .FirstOrDefault();
    }

    public bool IsDiskSpaceAvailable(string directory)
    {
        const double THRESHOLD = 2.0;

        DriveInfo driveInfo = new DriveInfo(directory);

        double freeSpace = driveInfo.AvailableFreeSpace;
        double totalSpace = driveInfo.TotalSize;
        double spaceRemaining = (freeSpace / totalSpace);

        if (spaceRemaining > THRESHOLD)
        {
            return true;
        }

        // _logger.LogError($"Disk space is too low. Disk space free: {(spaceRemaining * 100).ToString("###.##")}%"); // todo kr
        return false;
    }

    public void MoveFile(string sourceFilePath, string destinationDirectory)
    {
        File.Move(sourceFilePath, destinationDirectory);
    }

    public IEnumerable<string> GetFilesInDirectory(string directory)
    {
        return Directory.GetFiles(directory);
    }

    public IEnumerable<string> GetDirectoriesInDirectory(string directory)
    {
        return Directory.GetDirectories(directory);
    }

    public void PrepareAllFilesInDirectory(string directory)
    {
        var allFiles = GetFilesInDirectory(directory);

        foreach (string childDirectory in GetDirectoriesInDirectory(directory))
        {
            foreach (string childFile in GetFilesInDirectory(childDirectory))
            {
                MoveFile(
                    Path.Combine(childDirectory, childFile),
                    Path.Combine(directory, Path.GetFileName(childFile))
                );
            }
        }

        foreach (string file in GetFilesInDirectory(directory))
        {
            File.Move(
                file,
                Path.Combine(
                        directory,
                        Path.GetFileName(file)
                            .ToLower()
                            .Replace(";", "_")
                            .Replace(" ", "_")
                            .Replace("__", "_")
                            .Replace("\"", string.Empty)
                            .Replace("\'", string.Empty))
            );
        }
    }

    public bool DoesFileExist(string filePath)
    {
        return File.Exists(filePath);
    }

    public void DeleteFiles(string[] files)
    {
        foreach (var file in files)
        {
            DeleteFile(file);
        }
    }
    
}