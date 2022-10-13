using System.Text;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Subtitles;

internal abstract record BaseSrtSubtitle : BaseEntity
{
    internal BaseSrtSubtitle()
    {
        if (string.IsNullOrEmpty(BaseDirectory))
        {
            throw new SrtSubtitleBaseDirectoryIsNullOrEmptyException();
        }

        UploadDirectory = Path.Combine(BaseDirectory, "upload");
        IncomingDirectory = Path.Combine(BaseDirectory, "incoming");
        SrtOriginalText = string.Empty;
        BlogMarkdownText = string.Empty;
        SrtVideoText = string.Empty;
        BlogOutputFile = string.Empty;
        SubtitleOutputFile = string.Empty;
        SubTitleInputFile = string.Empty;
    }

    internal string BaseDirectory { get; init; }
    internal string UploadDirectory { get; }
    internal string IncomingDirectory { get; }
    internal string SrtOriginalText { get; private set; }
    internal string BlogMarkdownText { get; private set; }
    internal string SrtVideoText { get; private set; }
    internal string SubTitleInputFile { get; private set; }
    internal string SubtitleOutputFile { get; private set; }
    internal string BlogOutputFile { get; private set; }

    internal void SetSrtSubtitleText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new SrtSubtitleTextIsNullOrWhiteSpaceException();
        }

        string[] inputLines = SrtOriginalText.Split(Environment.NewLine);
        if (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true)
        {
            throw new SrtSubtitleContentsAreInvalidException();
        }

        SrtOriginalText = text;
    }

    internal void SetBlogMarkdownText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            throw new SrtSubtitleBlogMarkdownTextIsNullOrWhiteSpaceException();
        }

        BlogMarkdownText = text;
    }

    internal virtual void CleanSubtitle()
    {
        string[] inputLines = SrtOriginalText.Split('\n');
        int counter = 0;
        string videoString = string.Empty, blogString = string.Empty;
        StringBuilder videoSubtitle = new();
        StringBuilder blogPostText = new();

        foreach (var line in inputLines)
        {
            counter = counter >= 4 ? 1 : counter + 1;

            string cleanedLine = line
                .Replace("um", string.Empty)
                .Replace("uh", string.Empty)
                .Replace("[music] you", "[music]")
                .Replace("  ", " ")
                .Replace("all right", "alright")
                .Trim();

            videoSubtitle.Append(cleanedLine.ToUpper() + Environment.NewLine);

            if (counter == 3)
            {
                blogPostText.Append(cleanedLine + Environment.NewLine);
            }
        }

        SrtVideoText = videoSubtitle.ToString();
        BlogMarkdownText = blogPostText.ToString();

        RemoveDuplicateWordsFromBlogText();
        UpdateBlogTextToSentenceCase();
    }

    private void UpdateBlogTextToSentenceCase()
    {
        string[] inputLines = BlogMarkdownText.Split(". ");
        StringBuilder stringBuilder = new();

        foreach (var line in inputLines)
        {
            if (line.Length > 0)
            {
                stringBuilder.Append(line.Substring(0, 1).ToUpper() + line.Substring(1));
            }
        }

        BlogMarkdownText = stringBuilder.ToString();
    }

    private void RemoveDuplicateWordsFromBlogText()
    {
        string[] words = BlogMarkdownText.Split(Constants.Whitespace);
        string previousWord = string.Empty;
        StringBuilder stringBuilder = new();

        foreach (var word in words)
        {
            if (previousWord != word)
            {
                stringBuilder.Append(word + Constants.Whitespace);
            }

            previousWord = word;
        }

        BlogMarkdownText = stringBuilder.ToString();
    }

    internal void SetSubTitleFile(string? srtFile)
    {
        if (string.IsNullOrWhiteSpace(srtFile))
        {
            throw new SubTitleFileIsNullOrWhiteSpaceException();
        }

        SubTitleInputFile = srtFile;

        string filename = Path.GetFileName(srtFile);
        SubtitleOutputFile = Path.Combine(UploadDirectory, filename);
        BlogOutputFile = Path.Combine(UploadDirectory, filename.Replace(FileExtension.Srt, FileExtension.Md));
    }
}
