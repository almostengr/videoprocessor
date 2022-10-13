using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

public sealed class HandymanSubtitleService : BaseSubtitleService, IHandymanSubtitleService
{
    private readonly IFileSystemService _fileSystemService;

    public HandymanSubtitleService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                HandymanSubtitle subtitle = new();

                subtitle.SetSubTitleFile(_fileSystemService.GetRandomSrtFileFromDirectory(subtitle.IncomingDirectory));

                _fileSystemService.GetFileContents(subtitle.SubTitleInputFile);

                subtitle.CleanSubtitle();

                _fileSystemService.SaveFileContents(subtitle.SubtitleOutputFile, subtitle.SrtVideoText);
                _fileSystemService.SaveFileContents(subtitle.BlogOutputFile, subtitle.BlogMarkdownText);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

}