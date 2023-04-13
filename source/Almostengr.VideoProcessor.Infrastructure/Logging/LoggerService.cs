using Almostengr.VideoProcessor.Core.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Almostengr.VideoProcessor.Infrastructure.Logging;

public sealed class LoggerService<T> : ILoggerService<T>
{
    private readonly ILogger<T> _logger;

    public LoggerService(ILogger<T> logger)
    {
        _logger = logger;
    }

    public void LogError(Exception? exception, string message, params object[] args)
    {
        _logger.LogError(exception, message, args);
    }

    public void LogWarning(string message, params object[] args)
    {
        _logger.LogWarning(message, args);
    }

    public void LogInformation(string message, params object[] args)
    {
        _logger.LogInformation(message, args);
    }

    public void LogErrorProcessingFile(string fileName, Exception? exception)
    {
        LogError(exception, $"Error processing file {fileName}");
    }
}