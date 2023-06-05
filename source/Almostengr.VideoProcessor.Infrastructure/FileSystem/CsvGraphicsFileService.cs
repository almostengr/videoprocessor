using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class CsvGraphicsFileService : ICsvGraphicsFileService
{
    private readonly TimeSpan DisplayTime = new TimeSpan(0, 0, 5);

    public List<SubtitleFileEntry> ReadFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        List<SubtitleFileEntry> subtitles = new();

        foreach(var line in lines)
        {
            var parts = line.Split(",");
            TimeSpan startTime  = TimeSpan.Parse(parts[0]);
            TimeSpan endTime = startTime.Add(TimeSpan.FromSeconds(5));
            string text = parts[1].Trim();

            subtitles.Add(new SubtitleFileEntry(startTime, endTime, text));
        }

        return subtitles;
    }
}