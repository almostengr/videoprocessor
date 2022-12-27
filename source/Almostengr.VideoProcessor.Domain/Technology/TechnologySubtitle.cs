using Almostengr.VideoProcessor.Domain.Common.Entities;

namespace Almostengr.VideoProcessor.Domain.Technology;

internal sealed record TechnologySubtitle : BaseSrtSubtitle
{
    public TechnologySubtitle(string baseDirectory) : base(baseDirectory)
    {
    }
}