using Almostengr.VideoProcessor.Core.Common.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public sealed class AssSubtitleFile
{
    public string FilePath { get; init; }
    public string FileName { get; init; }
    public IList<SubtitleFileEntry> Subtitles { get; private set; }

    public AssSubtitleFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path not provided", nameof(filePath));
        }

        if (!filePath.ToLower().EndsWith(FileExtension.GraphicsAss.Value))
        {
            throw new ArgumentException("File path is not valid", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new ArgumentException("File does not exist");
        }

        FilePath = filePath;
        Subtitles = new List<SubtitleFileEntry>();
        FileName = Path.GetFileName(FilePath);
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