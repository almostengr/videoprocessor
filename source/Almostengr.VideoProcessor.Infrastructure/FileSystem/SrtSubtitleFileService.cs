using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Core.Common.Videos;

namespace Almostengr.VideoProcessor.Infrastructure.FileSystem;

public sealed class SrtSubtitleFileService : ISrtSubtitleFileService
{
    private const string TIME_SEPARATOR = " --> ";
    private readonly IFileSystemService _fileSystemService;

    public SrtSubtitleFileService(IFileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
    }

    public List<SubtitleFileEntry> ReadFile(string filePath)
    {
        List<SubtitleFileEntry> subtitles = new List<SubtitleFileEntry>();
        using (var reader = new StreamReader(filePath))
        {
            string line;
            int index = 0;
            TimeSpan startTime = new TimeSpan();
            TimeSpan endTime = new TimeSpan();
            string text = "";
            while ((line = reader.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (index != 0)
                    {
                        subtitles.Add(new SubtitleFileEntry(startTime, endTime, text));
                    }
                    index = 0;
                    startTime = new TimeSpan();
                    endTime = new TimeSpan();
                    text = "";
                }
                else if (index == 0)
                {
                    index++;
                }
                else if (index == 1)
                {
                    string[] times = line.Split(TIME_SEPARATOR);
                    startTime = TimeSpan.Parse(times[0]);
                    endTime = TimeSpan.Parse(times[1]);
                    index++;
                }
                else
                {
                    text += line + Environment.NewLine;
                }
            }
            
            if (index != 0)
            {
                subtitles.Add(new SubtitleFileEntry(startTime, endTime, text));
            }
        }
        return subtitles;
    }

    public void WriteFile(string filePath, IList<SubtitleFileEntry> subtitles)
    {
        const string TIME_FORMAT = @"hh\:mm\:ss\,fff";

        using (var writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < subtitles.Count; i++)
            {
                writer.WriteLine((i + 1).ToString());
                writer.WriteLine(subtitles[i].StartTime.ToString(TIME_FORMAT) + TIME_SEPARATOR + subtitles[i].EndTime.ToString(TIME_FORMAT));
                writer.WriteLine(subtitles[i].Text);
                writer.WriteLine();
            }
        }
    }
}