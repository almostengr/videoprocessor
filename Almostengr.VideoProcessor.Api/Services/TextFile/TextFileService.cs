using System;
using System.IO;
using Almostengr.VideoProcessor.Api.Configuration;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Api.Services.TextFile
{
    public class TextFileService : BaseService, ITextFileService
    {
        private readonly ILogger<TextFileService> _logger;

        public TextFileService(ILogger<TextFileService> logger, AppSettings appSettings) : base(logger, appSettings)
        {
            _logger = logger;
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

                if (Directory.Exists(directoryName) == false)
                {
                    base.CreateDirectory(directoryName);
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