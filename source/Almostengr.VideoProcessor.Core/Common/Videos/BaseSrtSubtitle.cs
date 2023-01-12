using System.Text;
using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseSrtSubtitle : BaseSubtitle
{
    protected BaseSrtSubtitle(string baseDirectory, string filePath) :
        base(baseDirectory, filePath)
    {
        if (filePath.EndsWith(FileExtension.Srt) == false)
        {
            throw new InvalidSubtitleFileException("File extension does not match expected type");
        }
    }

    internal override string SubtitleText()
    {
        string[] inputLines = FileContents.Split(Environment.NewLine);
        StringBuilder videoSubtitle = new();

        foreach (var line in inputLines)
        {
            videoSubtitle.Append(FixMisspellings(line).ToUpper() + Environment.NewLine);
        }

        return videoSubtitle.ToString();
    }

    internal override string BlogOutputFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Upload, FilePath.Replace(FileExtension.Srt, FileExtension.Md));
    }

    internal override string SubtitleOutputFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Upload, FilePath);
    }

    internal override void SetSubtitleText(string text)
    {
        string[] inputLines = text.Split(Environment.NewLine);
        if (inputLines[0].StartsWith("1") == false ||
            inputLines[1].StartsWith("00:") == false ||
            string.IsNullOrWhiteSpace(inputLines[2]))
        {
            throw new SrtSubtitleContentsAreInvalidException();
        }

        base.SetSubtitleText(text);
    }

    internal override string BlogPostText()
    {
        string[] subtitleLines = FileContents.Split(Environment.NewLine);
        int counter = 0;
        StringBuilder stringBuilder = new();

        foreach (var line in subtitleLines)
        {
            counter = counter >= 4 ? 1 : counter + 1;

            string cleanedLine = FixMisspellings(line);

            if (counter != 3)
            {
                continue;
            }

            stringBuilder.Append(cleanedLine + Environment.NewLine);
        }

        string[] words = stringBuilder.ToString().Split(Constant.Whitespace);
        stringBuilder.Clear();
        string previousWord = string.Empty;

        foreach (var currentWord in words)
        {
            if (previousWord != currentWord)
            {
                stringBuilder.Append(currentWord + Constant.Whitespace);
            }

            previousWord = currentWord;
        }

        string[] sentences = stringBuilder.ToString().Split(". ");
        stringBuilder.Clear();

        foreach (string sentence in sentences)
        {
            if (sentence.Length > 0)
            {
                stringBuilder.Append(sentence.Substring(0, 1).ToUpper() + sentence.Substring(1));
            }
        }

        const string rhtServicesWebsite = "[rhtservices.net](/)";
        return stringBuilder.ToString()
            .Replace("[music]", "(music)")
            .Replace("facebook", "<a href=\"https://www.facebook.com/rhtservicesllc/\" target=\"_blank\">Facebook</a>")
            .Replace("instagram", "<a href=\"https://www.instagram.com/rhtservicesllc/\" target=\"_blank\">Instagram</a>")
            .Replace("rhtservices.net", rhtServicesWebsite)
            .Replace("r h t services dot net", rhtServicesWebsite)
            .Replace("rht services", "RHT Services")
            ;
    }
}