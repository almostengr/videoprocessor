using Almostengr.VideoProcessor.Constants;
using Almostengr.VideoProcessor.DataTransferObjects;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services
{
    public abstract class BaseTranscriptService : BaseService, IBaseTranscriptService
    {
        protected BaseTranscriptService(ILogger<BaseService> logger) : base(logger)
        {
        }

        public string ProcessSentenceCase(string input)
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

        public string RemoveDupesFromBlogString(string blogText)
        {
            string[] words = blogText.Split(' ');
            string previousWord = string.Empty;
            string output = string.Empty;

            foreach (var word in words)
            {
                if (previousWord != word)
                {
                    output += word + Formatting.Space;
                }

                previousWord = word;
            }

            return output;
        }

        public virtual bool IsValidTranscript(TranscriptInputDto inputDto)
        {
            if (string.IsNullOrEmpty(inputDto.Input) == false)
            {
                return true;
            }

            return false;
        }
    } // end class
}