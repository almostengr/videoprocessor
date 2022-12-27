using System.Text;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common.Exceptions.Subtitles;

namespace Almostengr.VideoProcessor.Domain.Common.Entities;

internal abstract record BaseSrtSubtitle : BaseSubtitle
{
    internal BaseSrtSubtitle(string baseDirectory) : base(baseDirectory)
    {
    }

    internal override string GetBlogPostText()
    {
        string[] subtitleLines = SubtitleText.Split('\n');
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

        return stringBuilder.ToString();
    }

    internal override string GetSubtitleText()
    {
        string[] inputLines = SubtitleText.Split('\n');
        StringBuilder videoSubtitle = new();

        foreach (var line in inputLines)
        {
            videoSubtitle.Append(FixMisspellings(line).ToUpper() + Environment.NewLine);
        }

        return videoSubtitle.ToString();
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
}