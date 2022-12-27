using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class FileSystem : IFileSystem
{
    private readonly Random _random;
    private readonly AppSettings _appSettings;

    public FileSystem(AppSettings appSettings)
    {
        _random = new Random();
        _appSettings = appSettings;
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
            CreateDirectory(Path.GetDirectoryName(destination));
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
        IEnumerable<string> tarballPaths = GetFilesInDirectory(directory);

        if (tarballPaths.Count() == 0)
        {
            throw new NoTarballsPresentException();
        }

        return tarballPaths.Where(f => f.Contains(FileExtension.Tar))
            .Where(f => f.StartsWith(".") == false)
            .OrderBy(f => _random.Next())
            .Take(1)
            .First();
    }

    public string GetRandomSrtFileFromDirectory(string directory)
    {
        IEnumerable<string> srtFilePaths = GetFilesInDirectory(directory);

        if (srtFilePaths.Count() == 0)
        {
            throw new NoSrtFilesPresentException();
        }

        return srtFilePaths
            .Where(f => f.EndsWith(FileExtension.Srt))
            .OrderBy(f => _random.Next()).Take(1)
            .First();
    }

    public bool IsDiskSpaceAvailable(string directory)
    {
        DriveInfo driveInfo = new DriveInfo(directory);
        double spaceRemainingPercentage = (driveInfo.AvailableFreeSpace / driveInfo.TotalSize);

        if (spaceRemainingPercentage > _appSettings.DiskSpaceThreshold ||
            _appSettings.DiskSpaceThreshold == 0.0)
        {
            return true;
        }

        throw new DiskSpaceIsLowException($"Disk space remaining {spaceRemainingPercentage}");
    }

    public void MoveFile(string source, string destination, bool createDestinationDirectory = true)
    {
        try
        {
            File.Move(source, destination);
        }
        catch (DirectoryNotFoundException)
        {
            CreateDirectory(destination);
            File.Move(source, destination);
        }
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
        return File.ReadAllText(filePath).Trim();
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