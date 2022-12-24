using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;
using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

namespace Almostengr.VideoProcessor.Domain.Subtitles.TechnologySubtitle;

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

                subtitle.SetSubTitleFile(_fileSystem.GetRandomSrtFileFromDirectory(subtitle.IncomingDirectory));

                _fileSystem.GetFileContents(subtitle.SubTitleInputFile);

                subtitle.CleanSubtitle();

                _fileSystem.SaveFileContents(subtitle.SubtitleOutputFile, subtitle.SrtVideoText);
                _fileSystem.SaveFileContents(subtitle.BlogOutputFile, subtitle.BlogMarkdownText);
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