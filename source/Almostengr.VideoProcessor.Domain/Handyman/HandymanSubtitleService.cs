using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Subtitles;
using Almostengr.VideoProcessor.Domain.Common.Subtitles.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

public sealed class HandymanSubtitleService : BaseSubtitleService, IHandymanSubtitleService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILoggerService<HandymanSubtitleService> _logger;
    private readonly AppSettings _appSettings;

    public HandymanSubtitleService(IFileSystem fileSystemService, ILoggerService<HandymanSubtitleService> logger,
        AppSettings appSettings)
    {
        _fileSystem = fileSystemService;
        _logger = logger;
        _appSettings = appSettings;
    }

    public override async Task<bool> ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            HandymanSubtitle subtitle = new(_appSettings.HandymanDirectory);
            subtitle.SetSubtitleFilePath(_fileSystem.GetRandomSrtFileFromDirectory(subtitle.IncomingDirectory));
            subtitle.SetSubtitleText(_fileSystem.GetFileContents(subtitle.SubtitleInputFilePath));
            _fileSystem.SaveFileContents(subtitle.SubtitleOutputFilePath, subtitle.GetSubtitleText());
            _fileSystem.SaveFileContents(subtitle.BlogOutputFilePath, subtitle.GetBlogPostText());
            _fileSystem.MoveFile(subtitle.SubtitleInputFilePath, subtitle.SubtitleArchiveFilePath);
        }
        catch (NoSubtitleFilesPresentException)
        {
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return false;
    }
}