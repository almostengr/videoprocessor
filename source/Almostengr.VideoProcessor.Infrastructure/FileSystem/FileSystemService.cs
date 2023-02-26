using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Infrastructure.FileSystem.Exceptions;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;

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
            throw new VideoProcessorException("Directory cannot be null or whitespace");
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

    public string GetRandomFileByExtensionFromDirectory(string directory, FileExtension extension)
    {
        // IEnumerable<string> filePaths = GetFilesInDirectory(directory)

        try
        {
            // var filePath = GetFilesInDirectory(directory)
            //     .Where(f => f.ToLower().Contains(extension.Value) && !f.ToLower().EndsWith(FileExtension.Err.Value))
            //     .First();

            return GetFilesInDirectory(directory)
                .Where(f => f.ToLower().Contains(extension.Value) && !f.ToLower().EndsWith(FileExtension.Err.Value))
                .First();
        }
        catch (InvalidOperationException)
        {
            throw new NoFilesMatchException();
        }

        // if (filePaths.Count() == 0)
        // {
        //     throw new NoFilesMatchException();
        // }

        // return filePaths.OrderBy(f => _randomService.Next()).Take(1).First();
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

        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
        {
            throw new VideoProcessorException("Directories do not exist");
        }

        CreateDirectory(directoryName);
        File.WriteAllText(filePath, content);
    }

}