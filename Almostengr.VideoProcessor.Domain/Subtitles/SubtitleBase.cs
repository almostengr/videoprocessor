using Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Subtitles;

public abstract record SrtSubtitleBase
{
    public SrtSubtitleBase()
    {
        if (string.IsNullOrEmpty(BaseDirectory))
        {
            throw new SrtSubtitleBaseDirectoryIsNullOrEmptyException();
        }

        UploadDirectory = Path.Combine(BaseDirectory, "upload");
        IncomingDirectory = Path.Combine(BaseDirectory, "incoming");
        BlogWords = string.Empty;
        BlogText = string.Empty;
        VideoText = string.Empty;
    }

    public string BaseDirectory { get; init; }
    public string UploadDirectory { get; }
    public string IncomingDirectory { get; }
    public string BlogWords { get; private set; }
    public string BlogText { get; private set; }
    public string VideoText { get; private set; }

    public string ConvertToSentenceCase()
    {
        string[] inputLines = VideoText.Split(". ");
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

    public string CleanBlogString()
    {
        string rhtServicesWebsite = "[rhtservices.net](/)";

        return BlogText
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

    public string RemoveDuplicatesFromBlogString()
    {
        string[] words = BlogText.Split(' ');
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
}