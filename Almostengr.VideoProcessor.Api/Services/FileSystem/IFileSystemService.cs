using System.Threading.Tasks;

namespace Almostengr.VideoProcessor.Api.Services.FileSystem
{
    public interface IFileSystemService
    {
        void CreateDirectory(string directory);
        Task ConfirmFileTransferCompleteAsync(string videoArchive);
        void DeleteDirectory(string pathName);
        void DeleteDirectories(string[] directoryNames);
        void DeleteFiles(string[] fileNames);
        void DeleteFile(string pathName);
        bool DoesFileExist(string filePath);
        string[] GetDirectoriesInDirectory(string path);
        string[] GetFilesInDirectory(string path);
        string[] GetFilesInDirectory(string path, string searchPattern);
        bool IsDiskSpaceAvailable(string directory, double threshold);
        void MoveFile(string source, string destination);
        void CopyFile(string sourceFile, string destinationFIle);
    }
}
