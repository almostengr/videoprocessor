using System.Collections.Generic;
using Almostengr.VideoProcessor.Api.Constants;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class ChannelPropertiesDto
    {
        public string Name { get; set; }
        public string ArchiveDirectory { get; set; }
        public string WorkingDirectory { get; set; }
        public string UploadDirectory { get; set; }
        public string InputDirectory { get; set; }
        public string ClientSecretFileName { get; set; }
        public string DefaultDescription { get; set; }
        public List<string> ChannelLabels { get; set; }
    }
}