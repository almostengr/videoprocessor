namespace Almostengr.VideoProcessor.DataTransferObjects
{
    public class TranscriptInputDto
    {
        public string VideoTitle { get; set; } = string.Empty;
        public bool SaveFile { get; set; } = false;
        public string Input { get; set; }
    }
}