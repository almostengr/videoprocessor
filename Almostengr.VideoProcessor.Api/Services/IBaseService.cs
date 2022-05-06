namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IBaseService
    {
        bool IsDiskSpaceAvailable(string directory);
        void RemoveFile(string filename);
        void CreateDirectoryIfNotExists(string directory);
        void StartAndAwaitAsyncProcess(Process process, CancellationToken cancellationToken);
    }
}
