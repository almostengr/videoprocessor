namespace Almostengr.VideoProcessor.Core.VideoCommon
{
    public interface IVideoService
    {
        Task ExecuteServiceAsync(string videoArchive, CancellationToken cancellationToken);
        Task StartAsync(CancellationToken cancellationToken);
        bool IsDiskSpaceAvailable(string directory, double threshold);
        string GetRandomVideoArchiveInDirectory(string directory);
        Task WorkerIdleAsync(CancellationToken cancellationToken);
    }
}