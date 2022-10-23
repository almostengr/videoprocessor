using System.IO;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Videos;
using NUnit.Framework;
using Almostengr.VideoProcessor.Domain.Videos.Exceptions;

namespace Almostengr.VideoProcessor.Domain.Tests;

public class ToastmastersVideoServiceTests
{
    private ToastmastersVideo.ToastmastersVideo video;
    private const string BaseDirectory = "/tmp";

    [SetUp]
    public void Setup()
    {
        video = new(BaseDirectory);
    }

    [Test]
    public void BaseDirectory_IsSameAsRoot()
    {
        // setup 

        // attempt

        // verify
        Assert.AreEqual(video.BaseDirectory, BaseDirectory);
    }

    [Test]

    public void BaseDirectory_ThrowExceptionWhenWhiteSpace()
    {
        // verify
        Assert.Throws<VideoInvalidBaseDirectoryException>(() => {
            new ToastmastersVideo.ToastmastersVideo(Constants.Whitespace);
        });
    }

    [Test]
    public void IncomingDirectory_ContainsBaseWithIncoming()
    {
        Assert.AreEqual(video.IncomingDirectory, Path.Combine(BaseDirectory, DirectoryNames.Incoming));
    }

    [Test]
    public void ArchiveDirectory_ContainsBaseWithArchive()
    {
        Assert.AreEqual(video.ArchiveDirectory, Path.Combine(BaseDirectory, DirectoryNames.Archive));
    }

    [Test]
    public void UploadDirectory_ContainsBaseWithUpload()
    {
        Assert.AreEqual(video.UploadDirectory, Path.Combine(BaseDirectory, DirectoryNames.Upload));
    }

    [Test]
    public void TextOverlayHasBlueBackgroundAndWhiteText()
    {
        Assert.AreEqual(video.BoxColor(), FfMpegColors.SteelBlue);
        Assert.AreEqual(video.TextColor(), FfMpegColors.White);
    }
}