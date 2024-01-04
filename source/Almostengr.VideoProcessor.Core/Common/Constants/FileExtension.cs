namespace Almostengr.VideoProcessor.Core.Common.Constants;

public struct FileExtension
{
    public string Value { get; }
    public FileExtension(string value)
    {
        Value = value;
    }

    public static readonly FileExtension FfmpegInput = new FileExtension(".ffmpeginput");
    public static readonly FileExtension Jpg = new FileExtension(".jpg");
    public static readonly FileExtension Md = new FileExtension(".md");
    public static readonly FileExtension Mkv = new FileExtension(".mkv");
    public static readonly FileExtension Mov = new FileExtension(".mov");
    public static readonly FileExtension Mp3 = new FileExtension(".mp3");
    public static readonly FileExtension Mp4 = new FileExtension(".mp4");
    public static readonly FileExtension Srt = new FileExtension(".srt");
    public static readonly FileExtension ThumbTxt = new FileExtension(".thumbtxt");
}