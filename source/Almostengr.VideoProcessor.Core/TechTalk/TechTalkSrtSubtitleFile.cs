using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed record TechTalkSrtSubtitleFile : SrtSubtitleFile
{
    public TechTalkSrtSubtitleFile(string filePath) : base(filePath)
    {
    }

    public override string BlogPostText()
    {
        return base.BlogPostText().ToLower()
            .Replace("c sharp", "C#")
            .Replace("css", "CSS")
            .Replace("html", "HTML")
            .Replace("p h p", "PHP")
            .Replace("php", "PHP")
            .Trim();
    }
}