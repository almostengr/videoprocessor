using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services.FileSystem
{
    public interface IFileSystemService
    {
        bool IsDiskSpaceAvailable(string directory, double threshold);
        void DeleteDirectory(string pathName);
        void DeleteFile(string pathName);
        void CreateDirectory(string directory);
        void MoveFile(string source, string destination);
        string[] GetDirectoryContents(string path, string searchPattern);
        Task ConfirmFileTransferCompleteAsync(string videoArchive);
        string[] GetDirectoryContents(string path);
    }
}
