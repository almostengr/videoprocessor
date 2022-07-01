using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Core.Services.Subtitles
{
    public interface ISrtSubtitleService : ISubtitleService
    {
        void ArchiveSubtitleFile(string transcriptFilePath, string archiveDirectory);
        SubtitleOutputDto CleanSubtitle(SubtitleInputDto inputDto);
        string[] GetIncomingSubtitles(string directory);
        void SaveSubtitleFile(SubtitleOutputDto transcriptDto, string archiveDirectory);
        Task WorkerIdleAsync(CancellationToken stoppingToken);
        Task ExecuteAsync(CancellationToken stoppingToken);
        Task StartAsync(CancellationToken cancellationToken);
    }
}
