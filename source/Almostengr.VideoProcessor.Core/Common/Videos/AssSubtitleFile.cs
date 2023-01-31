namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record AssSubtitleFile
{
    public string FilePath { get; init; }
    public string FileName {get; init;}
    public IList<SubtitleFileEntry> Subtitles { get; private set; }

    public SrtSubtitleFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path not provided", nameof(filePath));
        }

        FilePath = filePath;
        Subtitles = new List<SubtitleFileEntry>();
        FileName  = Path.GetFileName(FilePath);
    }

    public void SetSubtitles(IList<SubtitleFileEntry> subtitles)
    {
        if (subtitles.Count() == 0)
        {
            throw new ArgumentException("At least one subtitle entry must be provided", nameof(subtitles));
        }

        Subtitles.Clear();
        Subtitles = subtitles;
    }
}