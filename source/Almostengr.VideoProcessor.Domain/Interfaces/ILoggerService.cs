namespace Almostengr.VideoProcessor.Domain.Interfaces;

public interface ILoggerService<T>
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception? exception, string message, params object[] args);
}