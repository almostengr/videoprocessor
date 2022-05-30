namespace Almostengr.VideoProcessor.DataTransferObjects
{
    public class SubtitleInputDto
    {
        public string VideoTitle { get; set; } = string.Empty;
        public bool SaveFile { get; set; } = false;
        public string Input { get; set; }
    }
}