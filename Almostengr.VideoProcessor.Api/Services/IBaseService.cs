namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IBaseService
    {
        bool IsDiskSpaceAvailable(string directory);
        void RemoveFile(string filename);
    }
}