using System.Text;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Subtitles.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Subtitles;

internal abstract record SrtSubtitleBase
{
    internal SrtSubtitleBase()
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
    }

    internal string BaseDirectory { get; init; }
    internal string UploadDirectory { get; }
    internal string IncomingDirectory { get; }
    internal string SrtOriginalText { get; private set; }
    internal string BlogMarkdownText { get; private set; }

    internal string SrtVideoText { get; private set; }
    internal string SubTitleFile { get; private set; }

    internal void SetSrtSubtitleText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new SrtSubtitleTextIsNullOrWhiteSpaceException();
        }

        string[] inputLines = SrtOriginalText.Split(Environment.NewLine);
        // return (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true);
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

    // internal bool IsValidSrtSubtitleFile()
    // {
    //     if (string.IsNullOrEmpty(SrtSubtitleText))
    //     {
    //         throw new SrtSubtitleContentsAreNullOrEmptyException();
    //     }

    //     string[] inputLines = SrtSubtitleText.Split(Environment.NewLine);
    //     // return (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true);
    //     if (inputLines[0].StartsWith("1") == true && inputLines[1].StartsWith("00:") == true)
    //     {
    //         throw new SrtSubtitleContentsAreInvalidException();
    //     }

    //     return true;
    // }

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

            // videoString += cleanedLine.ToUpper() + Environment.NewLine;
            videoSubtitle.Append(cleanedLine.ToUpper() + Environment.NewLine);

            if (counter == 3)
            {
                blogPostText.Append(cleanedLine + Environment.NewLine);
                // blogString += cleanedLine + Environment.NewLine;
            }
        }

        SrtVideoText = videoSubtitle.ToString();
        BlogMarkdownText = blogPostText.ToString();

        // blogString = RemoveDuplicatesFromBlogString(blogString);
        // blogString = CleanBlogString(blogString);
        // blogString = ConvertToSentenceCase(blogString);

        // SubtitleOutputDto outputDto = new();
        // outputDto.VideoTitle = inputDto.VideoTitle.Replace(FileExtension.Srt, string.Empty);
        // outputDto.VideoText = videoString;
        // outputDto.BlogText = blogString;
        // outputDto.BlogWords = blogString.Split(' ').Length;

        // return outputDto;

        RemoveDuplicateWordsFromBlogText();
        UpdateBlogTextToSentenceCase();
    }

    private void UpdateBlogTextToSentenceCase()
    {
        string[] inputLines = BlogMarkdownText.Split(". ");
        // string output = string.Empty;
        StringBuilder stringBuilder = new();

        foreach (var line in inputLines)
        {
            if (line.Length > 0)
            {
                // output += line.Substring(0, 1).ToUpper() + line.Substring(1);
                stringBuilder.Append(line.Substring(0, 1).ToUpper() + line.Substring(1));
            }
        }

        // return stringBuilder.ToString();
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

        SubTitleFile = srtFile;
    }
}
