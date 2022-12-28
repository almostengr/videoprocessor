using Almostengr.VideoProcessor.Domain.Common.Entities;

namespace Almostengr.VideoProcessor.Domain.Subtitles.HandymanSubtitle;

internal sealed record HandymanSubtitle : BaseSrtSubtitle
{
    public HandymanSubtitle(string baseDirectory) : base(baseDirectory)
    {
    }

    internal override string GetBlogPostText()
    {
        return base.GetBlogPostText().ToLower()
            .Replace("and so", string.Empty)
            .Replace("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YouTube</a>")
            .Trim();
    }
}
