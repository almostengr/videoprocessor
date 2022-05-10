using System;
using System.IO;
using Almostengr.VideoProcessor.Constants;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class VideoPropertiesDto
    {
        public string SourceTarFilePath { get; set; }
        public string VideoTitle { get { return Path.GetFileNameWithoutExtension(SourceTarFilePath).Replace($"{FileExtension.Tar}", ""); } }
        public string ArchiveTarFile
        {
            get
            {
                return $"{VideoTitle}.{DateTime.Now.ToString("yyyyMMdd")}.{DateTime.Now.ToString("HHmmss")}.tar.xz"; 
                // return Path.Combine(
                //     ArchiveDirectory,
                //     $"{VideoTitle}.{DateTime.Now.ToString("yyyyMMdd")}.{DateTime.Now.ToString("HHmmss")}.tar.xz"
                // );
            }
        }
        // public string ArchiveTarFilePath { get { return $"{VideoTitle}.tar.xz"; } }
        public string VideoDescription { get; set; }
        public string FfmpegInputFilePath { get; set; }
        public string OutputVideoFilePath
        {
            get
            {
                return Path.Combine(UploadDirectory, $"{VideoTitle}.mp4");
            }
        }
        public int VideoDurationSeconds { get; set; }
        public string VideoFilter { get; set; }
        public string WorkingDirectory { get; set; }
        public string UploadDirectory { get; set; }
        public string ArchiveDirectory { get; set; }
    }
}
