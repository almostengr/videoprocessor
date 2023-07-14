using Almostengr.VideoProcessor.Core.Common;

namespace Almostengr.VideoProcessor.Core.Transcriptions;

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