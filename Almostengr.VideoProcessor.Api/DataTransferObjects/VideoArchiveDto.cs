namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class VideoPropertiesDto
    {
        public string ArchiveFileName { get; set; }
        public string VideoTitle { 
            get
            {
                return ArchiveFileName.Split('.')[0];
            }
        }

        public string VideoDescription { get; set; }
        public string VideoKeywords { get; set; }
        public string VideoLocation { get; set; }
        public ChannelPropertiesDto ChannelProperties { get; set; }

    }
}