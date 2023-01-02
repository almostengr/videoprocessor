using System.IO;
using Almostengr.VideoProcessor.Domain.Common.Constants;
using Almostengr.VideoProcessor.Domain.Common;
using NUnit.Framework;
using Almostengr.VideoProcessor.Domain.Common.Videos.Exceptions;
using Almostengr.VideoProcessor.Domain.Videos.Handyman;

namespace Almostengr.VideoProcessor.Domain.Tests;

public class HandymanVideoTests
{
    private HandymanVideo video;
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
            new HandymanVideo(Constant.Whitespace);
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
    public void TextColor_ReturnsYellow()
    {
        // attempt
        string textColor = video.BannerTextColor();

        // verify
        Assert.AreEqual(textColor, FfMpegColors.RhtYellow);
    }

    [Test]
    public void BoxColor_ReturnsBlack()
    {
        string boxColor = video.BannerBackgroundColor();

        Assert.AreEqual(boxColor, FfMpegColors.Black);
    }

    [Test]
    public void ChannelBannerText_ReturnsRhtServices()
    {
        string channelText = video.ChannelBannerText();

        Assert.True(channelText.Contains("rhtservices") || channelText.Contains("Robinson Handy and Technology Services"));
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
    public void SetTarballFilePath_TarballFile_AreEqual()
    {
        string tarballFilePath = Path.Combine(video.IncomingDirectory, "anothertarball.tar.xz");

        video.SetTarballFilePath(tarballFilePath);

        Assert.AreEqual(tarballFilePath, video.TarballFilePath);
    }
}