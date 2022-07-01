using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Core.Services.Subtitles
{
    public static class SubtitleExtension
    {
        public static bool IsValidSrtSubtitleFile(this SubtitleInputDto inputDto)
        {
            if (string.IsNullOrEmpty(inputDto.Input))
            {
                return false;
            }

            string[] inputLines = inputDto.Input.Split('\n');
            return (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true);
        }
    }
}