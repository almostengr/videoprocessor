using Almostengr.VideoProcessor.Domain.Common.Entities;

namespace Almostengr.VideoProcessor.Domain.Technology;

internal sealed record TechnologySubtitle : BaseSrtSubtitle
{
    public TechnologySubtitle(string baseDirectory) : base(baseDirectory)
    {
    }

    internal override string GetBlogPostText()
    {
        return base.GetBlogPostText().ToLower()
            .Replace("c sharp", "C#")
            .Replace("css", "CSS")
            .Replace("html", "HTML")
            .Replace("p h p", "PHP")
            .Replace("php", "PHP")
            .Trim();
    }
}