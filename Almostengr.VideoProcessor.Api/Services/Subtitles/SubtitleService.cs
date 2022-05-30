using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.Subtitles
{
    public abstract class SubtitleService : ISubtitleService
    {
        protected SubtitleService(ILogger<SubtitleService> logger)
        {
        }

        public string ConvertToSentenceCase(string input)
        {
            string[] inputLines = input.Split(". ");
            string output = string.Empty;

            foreach (var line in inputLines)
            {
                if (line.Length > 0)
                {
                    output += line.Substring(0, 1).ToUpper() + line.Substring(1);
                }
            }

            return output;
        }

        public string CleanBlogString(string blogText)
        {
            string rhtServicesWebsite = "[rhtservices.net](/)";

            return blogText
                .Replace("  ", " ")
                .Replace("[music]", "(music)")
                .Replace("and so", string.Empty)
                .Replace("facebook", "<a href=\"https://www.facebook.com/rhtservicesllc/\" target=\"_blank\">Facebook</a>")
                .Replace("instagram", "<a href=\"https://www.instagram.com/rhtservicesllc/\" target=\"_blank\">Instagram</a>")
                .Replace("rhtservices.net", rhtServicesWebsite)
                .Replace("r h t services dot net", rhtServicesWebsite)
                .Replace("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YouTube</a>")
                .Trim();
        }

        public string RemoveDuplicatesFromBlogString(string blogText)
        {
            string[] words = blogText.Split(' ');
            string previousWord = string.Empty;
            string output = string.Empty;

            foreach (var word in words)
            {
                if (previousWord != word)
                {
                    output += word + " ";
                }

                previousWord = word;
            }

            return output;
        }

        public abstract bool IsValidFile(SubtitleInputDto inputDto);

    } // end class
}