using System;
using System.IO;
using Almostengr.VideoProcessor.Constants;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class VideoPropertiesDto
    {
        public VideoPropertiesDto(
            string sourceFilePath, string workingDirectory, string uploadDirectory, string archiveDirectory)
        {
            this.SourceTarFilePath = sourceFilePath;
            this.WorkingDirectory = workingDirectory;
            this.ArchiveDirectory = archiveDirectory;
            this.UploadDirectory = uploadDirectory;
        }

        public string SourceTarFilePath { get; set; }
        public string VideoTitle { get { return Path.GetFileNameWithoutExtension(SourceTarFilePath).Replace(FileExtension.Tar, string.Empty); } }
        public string ArchiveTarFile
        {
            get
            {
                return $"{VideoTitle.Replace(FileExtension.Mp4, string.Empty).Replace(FileExtension.Mkv, string.Empty)}.{DateTime.Now.ToString("yyyyMMdd")}.{DateTime.Now.ToString("HHmmss")}{FileExtension.TarXz}";
            }
        }
        public string OutputVideoFilePath
        {
            get
            {
                return Path.Combine(UploadDirectory, $"{VideoTitle.Replace(FileExtension.Mp4, string.Empty)}{FileExtension.Mp4}");
            }
        }
        public string VideoFilter { get; set; }
        public string WorkingDirectory { get; set; }
        public string UploadDirectory { get; set; }
        public string ArchiveDirectory { get; set; }
    }
}
