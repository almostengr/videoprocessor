using System.Text;
using Almostengr.VideoProcessor.Core.Constants;

namespace Almostengr.VideoProcessor.Core.Videos;

public abstract class TechnologyVideoProject : BaseVideoProject
{
    public TechnologyVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override List<string> BrandingTextOptions()
    {
        List<string> options = new();
        options.Add("Tech Talk with RHT Services");
        options.Add(Constant.ROBINSON_SERVICES);
        options.Add(Constant.RHT_WEBSITE);
        options.Add("@rhtservicestech");
        options.Add("#rhtservicestech");
        return options;
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Green;
    }

    internal string VideoGraphics(string brandingText)
    {
        StringBuilder stringBuilder = new(base.ChannelBrandDrawTextFilter(brandingText));

        stringBuilder.Append(Constant.CommaSpace);
        stringBuilder.Append(new DrawTextFilter(Title(), DrawTextFilterTextColor(), Opacity.Full,
                DrawTextFilterBackgroundColor(), Opacity.Medium, DrawTextPosition.LowerLeft).ToString());

        return stringBuilder.ToString();
    }

    public override string UploadDirectory()
    {
        return Path.Combine(BaseDirectory, "uploadtechnology");
    }

    public override string ArchiveDirectory()
    {
        return Path.Combine(BaseDirectory, "archivetechnology");
    }

    public override string ArchiveFilePath()
    {
        string filePath = Path.Combine(ArchiveDirectory(), FileName());
        return filePath;
    }

    public override string UploadFilePath()
    {
        string filePath = Path.Combine(UploadDirectory(), OutputFileName());
        return filePath;
    }
}


public sealed class TechTalkVideoProject : TechnologyVideoProject
{
    public TechTalkVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }
}

public sealed class LightShowVideoProject : TechnologyVideoProject
{
    public LightShowVideoProject(string filePath, string baseDirectory) : base(filePath, baseDirectory)
    {
    }

    public override FfMpegColor DrawTextFilterBackgroundColor()
    {
        return FfMpegColor.Maroon;
    }
}
