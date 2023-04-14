using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract class SrtSubtitleFile
{
    public string FilePath { get; init; }
    public IList<SubtitleFileEntry> Subtitles { get; private set; }

    public SrtSubtitleFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new ArgumentException(Constant.FileDoesNotExist, nameof(filePath));
        }

        if (!filePath.EndsWith(FileExtension.Srt.Value,  StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(Constant.FileTypeIsIncorrect, nameof(filePath));
        }

        FilePath = filePath;
        Subtitles = new List<SubtitleFileEntry>();
    }

    public string FileName()
    {
        return Path.GetFileName(FilePath);
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
        return Path.Combine(FilePath).Replace(FileExtension.Srt.Value, FileExtension.Md.Value, StringComparison.OrdinalIgnoreCase);
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