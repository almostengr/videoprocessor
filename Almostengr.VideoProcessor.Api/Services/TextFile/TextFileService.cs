using System;
using System.IO;
using Almostengr.VideoProcessor.Api.Services.FileSystem;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.TextFile
{
    public class TextFileService : ITextFileService
    {
        private readonly ILogger<TextFileService> _logger;
        private readonly IFileSystemService _fileSystem;

        public TextFileService(ILogger<TextFileService> logger, IFileSystemService fileSystem)
        {
            _logger = logger;
            _fileSystem = fileSystem;
        }

        public string GetFileContents(string filePath)
        {
            try
            {
                var fileStream = new FileStream(filePath, FileMode.Open);

                using (var streamReader = new StreamReader(fileStream))
                {
                    return streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return string.Empty;
            }
        }

        public void SaveFileContents(string filePath, string content)
        {
            try
            {
                var directoryName = Path.GetDirectoryName(filePath);
                _fileSystem.CreateDirectory(directoryName);

                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}