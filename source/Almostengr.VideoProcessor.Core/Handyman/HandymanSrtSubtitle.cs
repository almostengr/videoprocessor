using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed record HandymanSrtSubtitle : BaseSrtSubtitle
{
    public HandymanSrtSubtitle(string baseDirectory, string subtitleFileName) : base(baseDirectory, subtitleFileName)
    {
    }

    internal override string BlogPostText()
    {
        return base.BlogPostText().ToLower()
            .Replace("and so", string.Empty)
            .Replace("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YouTube</a>")
            .Trim();
    }
}