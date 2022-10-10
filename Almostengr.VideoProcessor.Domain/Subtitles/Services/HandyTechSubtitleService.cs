using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Services;

internal sealed class HandyTechSrtSubtitleService : BaseSubtitleService, ISubtitleService
{
    private readonly IFileSystemService _fileSystemService;

    public HandyTechSrtSubtitleService(IFileSystemService fileSystemService) : base(fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            HandyTechSrtSubtitle subtitle = new();

            subtitle.SetSubTitleFile(_fileSystemService.GetRandomSrtFileFromDirectory(subtitle.IncomingDirectory));
            
            _fileSystemService.GetFileContents(subtitle.SubTitleFile);

            subtitle.CleanSubtitle();

            _fileSystemService.SaveFileContents(subtitle.UploadDirectory, subtitle.SrtVideoText);
            _fileSystemService.SaveFileContents(subtitle.UploadDirectory, subtitle.BlogMarkdownText); // todo kr finish build out
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

}
