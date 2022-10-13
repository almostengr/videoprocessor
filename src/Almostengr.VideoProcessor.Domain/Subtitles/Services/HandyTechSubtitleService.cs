using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Services;

public sealed class HandyTechSrtSubtitleService : BaseSubtitleService, IHandyTechSrtSubtitleService
{
    private readonly IFileSystemService _fileSystemService;

    public HandyTechSrtSubtitleService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (true)
            {
                HandyTechSrtSubtitle subtitle = new();

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
    }

}
