namespace Almostengr.VideoProcessor.Api.DataTransferObjects
{
    public class VideoPropertiesDto
    {
        public string ArchiveFile { get; set; }
        public string VideoTitle { 
            get
            {
                return ArchiveFile.Split('.')[0];
            }
        }

        public string VideoDescription { get; set; }
        public string VideoKeywords { get; set; }
        public string VideoLocation { get; set; }
        public string IncomingArchiveFile { get; set; }
        public object UploadFile { get; set; }
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public int VideoDurationSeconds {get;set;}
        public string VideoFilter { get; internal set; }
    }
}
