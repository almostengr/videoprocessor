using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Core.TechTalk;

public sealed record TechTalkSrtSubtitle : BaseSrtSubtitle
{
    public TechTalkSrtSubtitle(string baseDirectory, string filePath) : base(baseDirectory, filePath)
    {
    }

    internal override string BlogPostText()
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