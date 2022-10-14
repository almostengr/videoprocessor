using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class FileSystem : IFileSystem
{
    private readonly Random _random;

    public FileSystem()
    {
        _random = new Random();
    }

    public void CreateDirectory(string directory)
    {
        if (Directory.Exists(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
    }

    public void CopyFile(string source, string destination, bool createDestinationDirectory = true)
    {
        if (createDestinationDirectory)
        {
            CreateDirectory(destination);
        }

        File.Copy(source, destination);
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

    public string GetRandomTarballFromDirectory(string directory)
    {
        return GetFilesInDirectory(directory)
            .Where(f => f.Contains(FileExtension.Tar))
            .Where(f => f.StartsWith(".") == false)
            .OrderBy(f => _random.Next()).Take(1)
            // .FirstOrDefault();
            .First();
    }

    public string GetRandomSrtFileFromDirectory(string directory)
    {
        return GetFilesInDirectory(directory)
            .Where(f => f.EndsWith(FileExtension.Srt))
            .Where(f => f.StartsWith(".") == false)
            .OrderBy(f => _random.Next()).Take(1)
            // .FirstOrDefault();
            .First();
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

        throw new DiskSpaceIsLowException();
    }

    public void MoveFile(string source, string destination, bool createDestinationDirectory = true)
    {
        if (createDestinationDirectory)
        {
            CreateDirectory(destination);
        }

        File.Move(source, destination);
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

    public string GetFileContents(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Open);

        using (var streamReader = new StreamReader(fileStream))
        {
            return streamReader.ReadToEnd();
        }
    }

    public void SaveFileContents(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new SaveFilePathIsNullOrEmptyException();
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new SaveFileContentIsNullOrEmptyException();
        }

        var directoryName = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directoryName))
        {
            throw new SaveFileDirectoryIsNullOrEmptyException();
        }

        CreateDirectory(directoryName);

        File.WriteAllText(filePath, content);
    }

}