using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.Handyman;

public sealed class HandymanSrtSubtitleFile : SrtSubtitleFile
{
    public HandymanSrtSubtitleFile(string filePath) : base(filePath)
    {
    }

    public override string BlogPostText()
    {
        return base.BlogPostText().ToString()
            .ReplaceIgnoringCase("ecobay", "Ecobee")
            .ReplaceIgnoringCase("facebook", "<a href=\"https://www.facebook.com/rhtservicesllc/\" target=\"_blank\">Facebook</a>")
            .ReplaceIgnoringCase("instagram", "<a href=\"https://www.instagram.com/rhtservicesllc/\" target=\"_blank\">Instagram</a>")
            .ReplaceIgnoringCase("Montgomery Alabama", "Montgomery, Alabama")
            .ReplaceIgnoringCase("youtube", "<a href=\"https://www.youtube.com/c/RobinsonHandyandTechnologyServices?sub_confirmation=1\" target=\"_blank\">YouTube</a>")
            ;
    }
}