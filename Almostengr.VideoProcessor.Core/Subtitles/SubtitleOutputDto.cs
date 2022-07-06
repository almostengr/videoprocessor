using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.DataTransferObjects
{
    public class SubtitleOutputDto : BaseDto
    {
        public string VideoTitle { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmss");
        public string BlogText { get; set; }
        public string VideoText { get; set; }
        public int BlogWords { get; set; }
    }
}