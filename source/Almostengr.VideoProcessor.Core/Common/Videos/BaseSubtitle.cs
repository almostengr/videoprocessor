using Almostengr.VideoProcessor.Core.Common.Constants;
using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

internal abstract record BaseSubtitle
{
    internal BaseSubtitle(string baseDirectory)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory))
        {
            throw new SubtitleBaseDirectoryIsNullOrEmptyException();
        }

        BaseDirectory = baseDirectory;
        ArchiveDirectory = Path.Combine(BaseDirectory, DirectoryName.Archive);
        UploadDirectory = Path.Combine(BaseDirectory, DirectoryName.Upload);
        IncomingDirectory = Path.Combine(BaseDirectory, DirectoryName.Incoming);
        SubtitleText = string.Empty;
        SubtitleInputFilePath = string.Empty;
        SubtitleOutputFilePath = string.Empty;
        BlogOutputFilePath = string.Empty;
        SubtitleArchiveFilePath = string.Empty;
    }

    internal string BaseDirectory { get; init; }
    internal string ArchiveDirectory { get; }
    internal string UploadDirectory { get; }
    internal string IncomingDirectory { get; }
    internal string SubtitleText { get; private set; }
    public string SubtitleInputFilePath { get; internal set; }
    public string SubtitleArchiveFilePath { get; internal set; }
    public string BlogOutputFilePath { get; internal set; }
    public string SubtitleOutputFilePath { get; internal set; }

    abstract internal string GetBlogPostText();
    abstract internal string GetSubtitleText();

    internal virtual void SetSubtitleText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new SubtitleTextIsNullOrWhitespaceException();
        }

        SubtitleText = input;
    }

    internal string FixMisspellings(string input)
    {
        return input
            .Replace("um", string.Empty)
            .Replace("uh", string.Empty)
            .Replace("[music] you", "[music]")
            .Replace(Constant.DoubleWhitespace, Constant.Whitespace)
            .Replace("all right", "alright")
            .Trim();
    }

    internal void SetSubtitleFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new InvalidSubtitleFileException();
        }

        SubtitleInputFilePath = filePath;
        SubtitleOutputFilePath = Path.Combine(UploadDirectory, Path.GetFileName(filePath));
        BlogOutputFilePath =
            Path.Combine(UploadDirectory, Path.GetFileNameWithoutExtension(filePath) + FileExtension.Md);
        SubtitleArchiveFilePath = Path.Combine(ArchiveDirectory, Path.GetFileName(filePath));
    }
}
