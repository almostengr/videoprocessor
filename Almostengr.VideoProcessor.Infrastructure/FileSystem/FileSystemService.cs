using Almostengr.VideoProcessor.Application.Common;
using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public class FileSystemService : IFileSystemService
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

    public string CreateFfmpegInputFile(string directory)
    {
        throw new NotImplementedException();
    }

    public void DeleteDirectory(string directory)
    {
        Directory.Delete(directory, true);
    }

    public Task ExtractTarball(string tarballFilePath, string extractDirectory)
    {
        throw new NotImplementedException();
    }

    public void ExtractTarballContents(string tarBall, string directory)
    {
        throw new NotImplementedException();
    }

    public string? GetRandomTarballFromDirectory(string directory)
    {
        return Directory.GetFiles(directory)
            .Where(x => x.Contains(FileExtension.Tar))
            .Where(x => x.StartsWith(".") == false)
            .OrderBy(x => _random.Next()).Take(1)
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

    public void RenderVideo(string directory, string ffmpegParameters)
    {
        throw new NotImplementedException();
    }
}