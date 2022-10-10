using Almostengr.VideoProcessor.Domain.Common;

namespace Almostengr.VideoProcessor.Domain.Subtitles.Services;

internal sealed record HandyTechSrtSubtitle : SrtSubtitleBase
{
    internal HandyTechSrtSubtitle()
    {
        BaseDirectory = Constants.HandyTechBaseDirectory;
    }

    internal override void CleanSubtitle()
    {
        base.CleanSubtitle();

        const string rhtServicesWebsite = "[rhtservices.net](/)";

        string text = BlogMarkdownText
            .Replace("  ", Constants.Whitespace)
            .Replace("[music]", "(music)")
            .Replace("and so", string.Empty)
            .Replace("facebook", "<a href=\"https://www.facebook.com/rhtservicesllc/\" target=\"_blank\">Facebook</a>")
            .Replace("instagram", "<a href=\"https://www.instagram.com/rhtservicesllc/\" target=\"_blank\">Instagram</a>")
            .Replace("rhtservices.net", rhtServicesWebsite)
            .Replace("r h t services dot net", rhtServicesWebsite)
            .Replace("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YouTube</a>")
            .Trim();

        SetBlogMarkdownText(text);
    }
}


// zinatta
// red lobster