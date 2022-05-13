using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.TextFile
{
    public class TextFileService : ITextFileService
    {
        private readonly ILogger<TextFileService> _logger;

        public TextFileService(ILogger<TextFileService> logger)
        {
            _logger = logger;
        }

        public void DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {fileName}");
            }
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
                var DirectoryName = Path.GetDirectoryName(filePath);

                if (Directory.Exists(DirectoryName) == false)
                {
                    Directory.CreateDirectory(DirectoryName);
                }

                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
    }
}