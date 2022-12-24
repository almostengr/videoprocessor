namespace Almostengr.VideoProcessor.Domain.Subtitles.TechnologySubtitle;

internal sealed record TechnologySubtitle : BaseSubtitle
{
    private string handymanDirectory;

    public TechnologySubtitle(string handymanDirectory)
    {
        this.handymanDirectory = handymanDirectory;
    }
}