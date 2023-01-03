namespace Almostengr.VideoProcessor.Domain.Common.Interfaces;

public interface IRandomService
{
    int Next(int minInt, int maxInt);
    int Next();
}