using Almostengr.VideoProcessor.Domain.Common.Entities;

namespace Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

internal sealed record HandymanSubtitle : BaseSrtSubtitle
{
    public HandymanSubtitle(string baseDirectory) : base(baseDirectory)
    {
    }

    internal override string GetBlogPostText()
    {
        const string rhtServicesWebsite = "[rhtservices.net](/)";
        return base.GetBlogPostText().ToLower()
            .Replace("[music]", "(music)")
            .Replace("and so", string.Empty)
            .Replace("facebook", "<a href=\"https://www.facebook.com/rhtservicesllc/\" target=\"_blank\">Facebook</a>")
            .Replace("instagram", "<a href=\"https://www.instagram.com/rhtservicesllc/\" target=\"_blank\">Instagram</a>")
            .Replace("rhtservices.net", rhtServicesWebsite)
            .Replace("r h t services dot net", rhtServicesWebsite)
            .Replace("rht services", "RHT Services")
            .Replace("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YouTube</a>")
            .Trim();
    }
}
