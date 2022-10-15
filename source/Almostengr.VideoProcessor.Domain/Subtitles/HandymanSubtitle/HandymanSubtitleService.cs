using Almostengr.VideoProcessor.Domain.Interfaces;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Common.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

public sealed class HandymanSubtitleService : BaseSubtitleService, IHandymanSubtitleService
{
    private readonly IFileSystem _fileSystem;
    private readonly ILoggerService<HandymanSubtitleService> _logger;

    public HandymanSubtitleService(IFileSystem fileSystemService, ILoggerService<HandymanSubtitleService> logger)
    {
        _fileSystem = fileSystemService;
        _logger = logger;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                HandymanSubtitle subtitle = new();

                subtitle.SetSubTitleFile(_fileSystem.GetRandomSrtFileFromDirectory(subtitle.IncomingDirectory));

                _fileSystem.GetFileContents(subtitle.SubTitleInputFile);

                subtitle.CleanSubtitle();

                _fileSystem.SaveFileContents(subtitle.SubtitleOutputFile, subtitle.SrtVideoText);
                _fileSystem.SaveFileContents(subtitle.BlogOutputFile, subtitle.BlogMarkdownText);
            }
        }
        catch (NoSrtFilesPresentException)
        {
            _logger.LogInformation(ExceptionMessage.NoSrtFilesPresent);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

}