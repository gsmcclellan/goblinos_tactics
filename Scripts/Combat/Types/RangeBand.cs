using System;
using System.Diagnostics;

namespace Goblinos.Scripts.Combat.Types;

public readonly struct RangeBand
{
    public RangeBand(int minRange, int maxRange)
    {
        Debug.Assert(minRange >= 0, $"RangeBand minRange < 0: {minRange}");
        Debug.Assert(maxRange >= minRange, $"RangeBand maxRange < minRange: min={minRange}, max={maxRange}");
        
        Min = Math.Max(0, minRange);
        Max = Math.Max(Min, maxRange);
    }

    public int Min { get; } = 1;
    public int Max { get; } = 1;

    public bool InRange(int distance)
    {
        return distance >= Min && distance <= Max;
    }
}