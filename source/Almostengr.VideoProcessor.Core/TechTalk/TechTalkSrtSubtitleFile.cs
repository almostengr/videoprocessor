using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed class TechTalkSrtSubtitleFile : SrtSubtitleFile
{
    public TechTalkSrtSubtitleFile(string filePath) : base(filePath)
    {
    }

    public override string BlogPostText()
    {
        return base.BlogPostText()
            .ReplaceIgnoringCase("c sharp", "C#")
            .ReplaceIgnoringCase("css", "CSS")
            .ReplaceIgnoringCase("html", "HTML")
            .ReplaceIgnoringCase("p h p", "PHP")
            .ReplaceIgnoringCase("php", "PHP")
            .Trim();
    }
}