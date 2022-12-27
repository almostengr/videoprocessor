namespace Almostengr.VideoProcessor.Domain.Common.Interfaces;

public interface ILoggerService<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception? exception, string message, params object[] args);
}