using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class SrtSubtitleFile
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

        if (!filePath.ToLower().EndsWith(FileExtension.Srt.Value))
        {
            throw new ArgumentException("File path is not valid", nameof(filePath));
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

    public string BlogFileName()
    {
        return Path.Combine(FilePath).Replace(FileExtension.Srt.Value, FileExtension.Md.Value);
    }

    public virtual string BlogPostText()
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (var subtitle in Subtitles)
        {
            stringBuilder.Append(subtitle.Text);
        }

        var words = stringBuilder.ToString().Split(" ");
        stringBuilder.Clear();
        string previousWord = string.Empty;

        foreach (string currentWord in words)
        {
            if (previousWord != currentWord)
            {
                stringBuilder.Append(currentWord + Constant.Whitespace);
            }

            previousWord = currentWord;
        }

        var sentences = stringBuilder.ToString().Split(". ");
        stringBuilder.Clear();

        foreach (string sentence in sentences)
        {
            if (sentence.Length > 0)
            {
                stringBuilder.Append(sentence.Substring(0, 1).ToUpper() + sentence.Substring(1));
            }
        }

        const string RHT_SERVICES_WEBSITE = "['rhtservices.net'](/)";

        return stringBuilder.ToString()
            .Replace("and so", string.Empty)
            .Replace("[music]", "(music)")
            .Replace("rhtservices.net", RHT_SERVICES_WEBSITE)
            .Replace("r h t services dot net", RHT_SERVICES_WEBSITE)
            .Replace("rht services", "RHT Services")
        ;
    }
}