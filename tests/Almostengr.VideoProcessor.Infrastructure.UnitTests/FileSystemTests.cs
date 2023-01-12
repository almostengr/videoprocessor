using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using NUnit.Framework;
using Almostengr.VideoProcessor.Infrastructure.Common;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;

namespace Almostengr.VideoProcessor.Infrastructure.UnitTests;

public class FileSystemTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateDirectory_ReturnsNothing()
    {
        AppSettings appSettings = new AppSettings();
        IRandomService randomService = new RandomService();
        IFileSystemService fileSystem = new FileSystemService(appSettings, randomService);

        fileSystem.CreateDirectory("/tmp");
    }

}