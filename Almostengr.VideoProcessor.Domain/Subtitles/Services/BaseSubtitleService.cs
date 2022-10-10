using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Services;

internal abstract class BaseSubtitleService : ISubtitleService
{
    private readonly IFileSystemService _fileSystemService;

    internal BaseSubtitleService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public abstract Task ExecuteAsync(CancellationToken stoppingToken);
    // public abstract void GetFileContents(string filePath);
    // public abstract void SaveFileContents(string filePath, string content);
}