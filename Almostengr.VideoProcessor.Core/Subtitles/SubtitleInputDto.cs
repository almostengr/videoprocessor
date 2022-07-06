using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.DataTransferObjects
{
    public class SubtitleInputDto : BaseDto
    {
        public SubtitleInputDto() {}

        public SubtitleInputDto(string input, string filename)
        {
            Input = input;
            filename = VideoTitle;
        }

        public string VideoTitle { get; set; } = string.Empty;
        public bool SaveFile { get; set; } = false;
        public string Input { get; set; }
    }
}