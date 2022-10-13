using Almostengr.VideoProcessor.Domain.Interfaces;

namespace Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

public sealed class HandymanSubtitleService : BaseSubtitleService, IHandymanSubtitleService
{
    private readonly IFileSystem _fileSystem;

    public HandymanSubtitleService(IFileSystem fileSystemService)
    {
        _fileSystem = fileSystemService;
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
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Task.CompletedTask;
    }

}