using System.Collections;
using System.Collections.Generic;
using Almostengr.VideoProcessor.Core.Common;
using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Almostengr.VideoProcessor.Infrastructure.FileSystem;
using NUnit.Framework;

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
        IFileSystemService fileSystem = new FileSystem.FileSystem(appSettings);

        fileSystem.CreateDirectory("/tmp");
    }

}