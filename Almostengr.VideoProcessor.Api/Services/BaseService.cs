using System.IO;

namespace Almostengr.VideoProcessor.Api.Services
{
    public abstract class BaseService : IBaseService
    {
        public bool IsDiskSpaceAvailable(string directory)
        {
            var freeSpace = new DriveInfo(directory).AvailableFreeSpace;
            var totalSpace = new DriveInfo(directory).TotalSize;
            return (freeSpace / totalSpace) > 0.05;
        }
    }
}