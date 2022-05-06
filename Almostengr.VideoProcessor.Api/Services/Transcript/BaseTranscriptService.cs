using Almostengr.VideoProcessor.Constants;
using Almostengr.VideoProcessor.DataTransferObjects;

namespace Almostengr.VideoProcessor.Api.Services
{
    public abstract class BaseTranscriptService : IBaseTranscriptService
    {
        public string ProcessSentenceCase(string input)
        {
            string[] inputLines = input.Split(". ");
            string output = string.Empty;

            foreach(var line in inputLines)
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
            return blogText
                .Replace("  ", " ")
                .Replace("[music]", "(music)")
                .Replace("and so ", string.Empty)
                .Replace("facebook", "<a href=\"https://www.facebook.com/rhtservicesllc/\" target=\"_blank\">FACEBOOK</a>")
                .Replace("instagram", "<a href=\"https://www.instagram.com/rhtservicesllc/\" target=\"_blank\">INSTAGRAM</a>")
                .Replace("rhtservices.net", "[RHTSERVICES.NET](/)")
                .Replace("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YOUTUBE</a>")
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