namespace Almostengr.VideoProcessor.Domain.Common;

public static class Constants
{
    public const string Whitespace = " ";
    public const string CommaSpace = ", ";
    public const string ChristmasLightShow = "christmas light show";
    public const string FfmpegInputFileName = "ffmpeginput.txt";
}

public static class DirectoryNames
{
    public const string Incoming = "incoming";
    public const string Working = "working";
    public const string Archive = "archive";
    public const string Upload = "upload";
}

public static class FileExtension
{
    public const string Jpg = ".jpg";
    public const string Md = ".md";
    public const string Mov = ".mov";
    public const string Mkv = ".mkv";
    public const string Mp3 = ".mp3";
    public const string Mp4 = ".mp4";
    public const string Png = ".png";
    public const string Srt = ".srt";
    public const string Tar = ".tar";
    public const string TarGz = ".tar.gz";
    public const string TarXz = ".tar.xz";
    public const string Ts = ".ts";
    public const string Txt = ".txt";
}

public static class ExceptionMessage
{
    public const string NoTarballsPresent = "No tarballs are present";
    public const string NoSrtFilesPresent = "No SRT subtitles are present";
}
