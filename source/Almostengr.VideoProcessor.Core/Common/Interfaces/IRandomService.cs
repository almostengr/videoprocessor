namespace Almostengr.VideoProcessor.Core.Common.Interfaces;

public interface IRandomService
{
    int Next(int minInt, int maxInt);
    int Next();
    int SubscribeLikeDuration();
}