namespace Almostengr.VideoProcessor.Core.Common.Constants;

public struct FileExtension
{
    public string Value { get; }
    public FileExtension(string value)
    {
        Value = value;
    }

    public static readonly FileExtension DraftTar = new FileExtension(".draft.tar");
    public static readonly FileExtension Err = new FileExtension(".err");
    public static readonly FileExtension FfmpegInput = new FileExtension(".ffmpeginput");
    public static readonly FileExtension GraphicsAss = new FileExtension(".graphics.ass");
    public static readonly FileExtension Jpg = new FileExtension(".jpg");
    public static readonly FileExtension Kdenlive = new FileExtension(".kdenlive");
    public static readonly FileExtension Md = new FileExtension(".md");
    public static readonly FileExtension Mkv = new FileExtension(".mkv");
    public static readonly FileExtension Mov = new FileExtension(".mov");
    public static readonly FileExtension Mp3 = new FileExtension(".mp3");
    public static readonly FileExtension Mp4 = new FileExtension(".mp4");
    public static readonly FileExtension Ready = new FileExtension(".ready");
    public static readonly FileExtension StreetsCsv = new FileExtension(".streets.csv");
    public static readonly FileExtension Srt = new FileExtension(".srt");
    public static readonly FileExtension Tar = new FileExtension(".tar");
    public static readonly FileExtension TarGz = new FileExtension(".tar.gz");
    public static readonly FileExtension TarXz = new FileExtension(".tar.xz");
    public static readonly FileExtension ThumbTxt = new FileExtension(".thumbtxt");
    public static readonly FileExtension Ts = new FileExtension(".ts");
}