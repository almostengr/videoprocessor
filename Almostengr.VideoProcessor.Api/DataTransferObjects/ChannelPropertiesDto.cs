using System.Collections.Generic;

namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class ChannelPropertiesDto
    {
        public string Name { get; set; }
        public string ClientSecretFileName { get; set; }
        public string DefaultDescription { get; set; }
        public List<string> ChannelLabels { get; set; }
    }
}