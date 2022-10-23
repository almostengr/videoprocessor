using System.IO;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Videos;
using NUnit.Framework;
using Almostengr.VideoProcessor.Domain.Videos.Exceptions;
using Almostengr.VideoProcessor.Domain.Toastmasters;

namespace Almostengr.VideoProcessor.Domain.Tests;

public class ToastmastersVideoTests
{
    private ToastmastersVideo video;
    private const string BaseDirectory = "/tmp";

    [SetUp]
    public void Setup()
    {
        video = new(BaseDirectory);
    }

    [Test]
    public void CreateNewInstanceOfClass_EqualsBaseDirectory()
    {
        // setup 

        // attempt

        // verify
        Assert.AreEqual(BaseDirectory, video.BaseDirectory);
    }

    [Test]

    public void BaseDirectory_ThrowExceptionWhenWhiteSpace()
    {
        // verify
        Assert.Throws<VideoInvalidBaseDirectoryException>(() =>
        {
            new ToastmastersVideo(Constants.Whitespace);
        });
    }

    [Test]
    public void IncomingDirectory_EqualsBaseWithIncoming()
    {
        string expectedDirectory = Path.Combine(BaseDirectory, DirectoryNames.Incoming);

        Assert.AreEqual(expectedDirectory, video.IncomingDirectory);
    }

    [Test]
    public void ArchiveDirectory_EqualsBaseWithArchive()
    {
        string expectedDirectory = Path.Combine(BaseDirectory, DirectoryNames.Archive);

        Assert.AreEqual(expectedDirectory, video.ArchiveDirectory);
    }

    [Test]
    public void UploadDirectory_EqualsBaseWithUpload()
    {
        string expectedDirectory = Path.Combine(BaseDirectory, DirectoryNames.Upload);

        Assert.AreEqual(expectedDirectory, video.UploadDirectory);
    }

    [Test]
    public void TextColor_ReturnsWhite()
    {
        // attempt
        string textColor = video.TextColor();

        // verify
        Assert.AreEqual(textColor, FfMpegColors.White);
    }

    [Test]
    public void BoxColor_ReturnsSteelBlue()
    {
        string boxColor = video.BoxColor();

        Assert.AreEqual(boxColor, FfMpegColors.SteelBlue);
    }

    [Test]
    public void ChannelBannerText_ReturnEqualsTowerToastmastersDotOrg()
    {
        string channelText = video.ChannelBannerText();

        Assert.AreEqual(channelText, "towertoastmasters.org");
    }

    [Test]
    public void SetTarballFilePath_EmptyString_ThrowsException()
    {
        string tarballFilePath = string.Empty;

        Assert.Throws<VideoTarballFilePathIsNullOrEmptyException>(() =>
        {
            video.SetTarballFilePath(tarballFilePath);
        });
    }

    [Test]
    public void SetTarballFilePath_TextFile_ThrowsException()
    {
        string tarballFilePath = "testing.txt";

        Assert.Throws<VideoTarballFilePathHasWrongExtensionException>(() =>
        {
            video.SetTarballFilePath(tarballFilePath);
        });
    }

    [Test]
    public void SetTarballFilePath_TarballFile_ThrowsException()
    {
        string tarballFilePath = Path.Combine(video.IncomingDirectory, "anothertarball.tar.xz");

        Assert.Throws<VideoTarballFileDoesNotExistException>(() => 
        {
            video.SetTarballFilePath(tarballFilePath);
        });
    }
}