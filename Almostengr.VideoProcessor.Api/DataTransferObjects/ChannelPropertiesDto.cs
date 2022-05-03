using System.Collections.Generic;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class ChannelPropertiesDto
    {
        public string Name { get; set; }
        public string ArchiveDirectory { get; set; }
        public string WorkingDirectory { get; set; }
        public string UploadDirectory { get; set; }
        public string InputDirectory { get; set; }

        public string FfmpegInputFile
        {
            get { return WorkingDirectory + "/input.txt"; }
        }

        public string DestinationFile
        {
            get { return WorkingDirectory + "/destination.txt"; }
        }

        public string SubtitlesFile
        {
            get { return WorkingDirectory + "/subtitles.ass"; }
        }

        public string MajorRoadsFile
        {
            get { return WorkingDirectory + "/majorroads.txt"; }
        }

        public string ClientSecretFileName { get; set; }
        public string DefaultDescription { get; set; }
        public List<string> ChannelLabels { get; set; }
    }
}