using System.Text.RegularExpressions;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class AssSubtitleFileService : IAssSubtitleFileService
{
    public AssSubtitleFileService()
    {
    }

    public List<SubtitleFileEntry> ReadFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        // Use regex to extract the dialogue lines
        Regex dialogueRegex = new Regex("Dialogue:.*");
        var dialogueLines = lines.Where(line => dialogueRegex.IsMatch(line));

        return dialogueLines.Select(line =>
        {
            string[] parts = line.Split(',');
            TimeSpan startTime = TimeSpan.Parse(parts[1]);
            TimeSpan endTime = TimeSpan.Parse(parts[2]);
            string text = parts[9];

            return new SubtitleFileEntry(startTime, endTime, text);
        })
        .ToList();
    }
}