using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Common.Interfaces;
using Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;
using Almostengr.VideoProcessor.Domain.Common.Services;

namespace Almostengr.VideoProcessor.Domain.Technology;

public sealed class TechnologySubtitleService : BaseSubtitleService, ITechnologySubtitleService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILoggerService<HandymanSubtitleService> _logger;
    private readonly AppSettings _appSettings;

    public TechnologySubtitleService(IFileSystem fileSystemService, ILoggerService<HandymanSubtitleService> logger,
        AppSettings appSettings)
    {
        _fileSystem = fileSystemService;
        _logger = logger;
        _appSettings = appSettings;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                TechnologySubtitle subtitle = new(_appSettings.TechnologyDirectory);

                subtitle.SetSubtitleFilePath(_fileSystem.GetRandomSrtFileFromDirectory(subtitle.IncomingDirectory));

                string contents = _fileSystem.GetFileContents(subtitle.SubtitleInputFilePath);
                subtitle.SetSubtitleText(contents);

                _fileSystem.SaveFileContents(subtitle.SubtitleOutputFilePath, subtitle.GetSubtitleText());
                _fileSystem.SaveFileContents(subtitle.BlogOutputFilePath, subtitle.GetBlogPostText());
            }
        }
        catch (NoSrtFilesPresentException)
        { }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }

        return Task.CompletedTask;
    }
}