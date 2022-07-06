using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Core.Subtitles
{
    public interface ISrtSubtitleService : ISubtitleService
    {
        void ArchiveSubtitleFile(string transcriptFilePath, string archiveDirectory);
        SubtitleOutputDto CleanSubtitle(SubtitleInputDto inputDto);
        string[] GetIncomingSubtitles(string directory);
        void SaveSubtitleFile(SubtitleOutputDto transcriptDto, string archiveDirectory);
        Task WorkerIdleAsync(CancellationToken stoppingToken);
        Task ExecuteServiceAsync(string subtitleFile, CancellationToken stoppingToken);
        Task StartAsync(CancellationToken cancellationToken);
        string GetRandomSubtitleFile(string directory);
    }
}
