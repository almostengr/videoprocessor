using Almostengr.VideoProcessor.Core.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Common.Videos;

public abstract record BaseSubtitle
{
    protected BaseSubtitle(string baseDirectory, string filePath)
    {
        if (string.IsNullOrWhiteSpace(baseDirectory) || string.IsNullOrWhiteSpace(filePath))
        {
            throw new VideoProcessorException("Subtitle arguments are not valid");
        }

        BaseDirectory = baseDirectory;
        FilePath = filePath;
        FileContents = string.Empty;
    }

    public string FilePath { get; init; }
    public string FileContents { get; private set; }
    public string BaseDirectory { get; init; }

    internal abstract string SubtitleOutputFilePath();
    internal abstract string BlogPostText();
    internal abstract string SubtitleText();
    internal abstract string BlogOutputFilePath();
    
    internal virtual string IncomingFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Incoming, FileName());
    }

    internal virtual string ArchiveFilePath()
    {
        return Path.Combine(BaseDirectory, DirectoryName.Archive, FileName());
    }
    
    internal virtual void SetSubtitleText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new SubtitleTextIsNullOrWhitespaceException();
        }

        FileContents = input;
    }

    internal string FileName()
    {
        return Path.GetFileName(FilePath);
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
}