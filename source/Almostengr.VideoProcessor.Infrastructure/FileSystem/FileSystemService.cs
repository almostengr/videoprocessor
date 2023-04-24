using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;
using Almostengr.VideoProcessor.Core.Common.Interfaces;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class FileSystemService : IFileSystemService
{
    private readonly IRandomService _randomService;
    private readonly AppSettings _appSettings;

    public FileSystemService(AppSettings appSettings, IRandomService randomService)
    {
        _randomService = randomService;
        _appSettings = appSettings;
    }

    public void CreateDirectory(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentException("Directory cannot be null or whitespace");
        }

        if (Directory.Exists(directory))
        {
            return;
        }

        Directory.CreateDirectory(directory);
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

    public string? GetRandomFileByExtensionFromDirectory(string directory, FileExtension extension)
    {
        return GetFilesInDirectory(directory)
            .Where(f => f.ContainsIgnoringCase(extension.Value) && !f.EndsWithIgnoringCase(FileExtension.Err.Value))
            .FirstOrDefault();
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

    public void MoveFile(string source, string destination)
    {
        try
        {
            if (File.Exists(destination))
            {
                DeleteFile(destination);
            }

            File.Move(source, destination);
        }
        catch (DirectoryNotFoundException)
        {
            CreateDirectory(Path.GetDirectoryName(destination) ?? throw new VideoProcessorException("Directory is null"));
            File.Move(source, destination);
        }
    }

    public FileInfo[] GetFilesInDirectoryWithInfo(string directory)
    {
        return (new DirectoryInfo(directory)).GetFiles();
    }

    public IEnumerable<string> GetTarballFilesInDirectory(string directory)
    {
        return Directory.GetFiles(directory)
            .Where(f => f.EndsWithIgnoringCase(FileExtension.TarGz.Value) ||
                        f.EndsWithIgnoringCase(FileExtension.TarXz.Value) ||
                        f.EndsWithIgnoringCase(FileExtension.Tar.Value));
    }

    public IEnumerable<string> GetFilesInDirectory(string directory)
    {
        return Directory.GetFiles(directory);
    }

    public FileInfo[] GetFilesInDirectoryWithFileInfo(string directory)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(directory);
        return directoryInfo.GetFiles();
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
                            .Replace(";", "_")
                            .Replace(" ", "_")
                            )
            );
        }
    }

    public void SaveFileContents(string filePath, string content)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is not valid", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Contents is null or white space", nameof(content));
        }

        string directoryName =
            Path.GetDirectoryName(filePath) ?? throw new VideoProcessorException("Directory cannot be null");

        CreateDirectory(directoryName);
        File.WriteAllText(filePath, content);
    }

    public IEnumerable<FileInfo> GetVideoFilesInDirectoryWithFileInfo(string directory)
    {
        return GetFilesInDirectoryWithFileInfo(directory)
            .Where(f => f.FullName.EndsWithIgnoringCase(FileExtension.Mov.Value) ||
                        f.FullName.EndsWithIgnoringCase(FileExtension.Mkv.Value) ||
                        f.FullName.EndsWithIgnoringCase(FileExtension.Mp4.Value));
    }
}