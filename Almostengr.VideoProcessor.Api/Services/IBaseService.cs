using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services
{
    public interface IBaseService
    {
        bool IsDiskSpaceAvailable(string directory);
        void DeleteDirectory(string pathName);
        void DeleteFile(string pathName);
        void CreateDirectory(string directory);
        void MoveFile(string source, string destination);
        string[] GetDirectoryContents(string path, string searchPattern);
        Task ConfirmFileTransferCompleteAsync(string videoArchive);
    }
}
