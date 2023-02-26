using Almostengr.VideoProcessor.Core.Common.Interfaces;

namespace Almostengr.VideoProcessor.Infrastructure.Common;

public sealed class RandomService : IRandomService
{
    private readonly Random _random;

    public RandomService()
    {
        _random = new Random();
    }

    public int Next(int minInt, int maxInt)
    {
        return _random.Next(minInt, maxInt);
    }
    
}
