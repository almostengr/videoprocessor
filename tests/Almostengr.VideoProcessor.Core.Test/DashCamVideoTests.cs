using NUnit.Framework;
using Almostengr.VideoProcessor.Core.DashCam;

namespace Almostengr.VideoProcessor.Core.Test;

public class DashCamVideoTests
{
    DashCamVideoFile? video = null;

    // [OneTimeSetUp]
    // public void OneTimeSetup()
    // {
    //     video = new DashCamVideoFile("/tmp", "testfile.tar");
    // }

    // [Test]
    // public void DashCamVideo_BrandingOptions_ReturnsKennyRamDashCam()
    // {
    //     var textOptions = video.BrandingTextOptions();

    //     Assert.True(textOptions.Length == 1);
    //     Assert.True(textOptions[0] == "Kenny Ram Dash Cam");
    // }

    // [Test]
    // public void DashCamVideo_BaseDirectory_ReturnsSlashTmp()
    // {
    //     string baseDirectory = video.BaseDirectory;

    //     Assert.AreEqual(baseDirectory, "/tmp");
    // }

    // [Test]
    // public void DashCamVideo_AchiveFileNameProperty_ReturnsTestFileTar()
    // {
    //     var archiveFileName = video.TarballFileName;

    //     Assert.True(archiveFileName == "testfile.tar");
    // }
}