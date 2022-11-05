using System.Collections;
using System.Collections.Generic;
using Almostengr.VideoProcessor.Domain.Common;
using Almostengr.VideoProcessor.Domain.Interfaces;
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
        IFileSystem fileSystem = new FileSystem.FileSystem(appSettings);

        fileSystem.CreateDirectory("/tmp");
    }

}