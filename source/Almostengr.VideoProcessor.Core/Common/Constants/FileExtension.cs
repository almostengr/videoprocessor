namespace Almostengr.VideoProcessor.Core.Common.Constants;

public struct FileExtension
{
    private string Value;

    public FileExtension(string value)
    {
        Value = value;
    }

    public static readonly FileExtension AudioMkv = new FileExtension(".audio.mkv");
    public static readonly FileExtension AudioMp4 = new FileExtension(".audio.mp4");
    public static readonly FileExtension Avi = new FileExtension(".avi");
    public static readonly FileExtension DraftTar = new FileExtension(".draft.tar");
    public static readonly FileExtension Err = new FileExtension(".err"); 
    public static readonly FileExtension GraphicsAss = new FileExtension(".graphics.ass");
    public static readonly FileExtension Jpg = new FileExtension(".jpg");
    public static readonly FileExtension Kdenlive = new FileExtension(".kdenlive");
    public static readonly FileExtension Log = new FileExtension(".log");
    public static readonly FileExtension Md = new FileExtension(".md");
    public static readonly FileExtension Mkv = new FileExtension(".mkv");
    public static readonly FileExtension Mov = new FileExtension(".mov");
    public static readonly FileExtension Mp3 = new FileExtension(".mp3");
    public static readonly FileExtension Mp4 = new FileExtension(".mp4");
    public static readonly FileExtension Png = new FileExtension(".png");
    public static readonly FileExtension Srt = new FileExtension(".srt");
    public static readonly FileExtension Tar = new FileExtension(".tar");
    public static readonly FileExtension TarGz = new FileExtension(".tar.gz");
    public static readonly FileExtension TarXz = new FileExtension(".tar.xz");
    public static readonly FileExtension TmpMp4 = new FileExtension(".tmp.mp4");
    public static readonly FileExtension Ts = new FileExtension(".ts");
    public static readonly FileExtension Txt = new FileExtension(".txt");

    public override string ToString()
    {
        return Value.ToString();
    }
}