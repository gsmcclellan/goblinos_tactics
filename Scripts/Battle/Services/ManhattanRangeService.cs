#nullable enable
using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

/// <summary>
/// Provides cached Manhattan-range offset patterns and helpers to apply them to origin cells.
/// This is action-agnostic (Attack/Ability/etc. decide what these cells mean).
/// </summary>
public sealed class ManhattanRangeService
{
    private static readonly Logger Logger = LogManager.For<ManhattanRangeService>();

    private static readonly Dictionary<(int MinRange, int MaxRange), Vector2I[]> _cachedPatterns = new();

    public static Vector2I[] GetPattern(int minRange, int maxRange)
    {
        Logger.Log("GetPattern", LogSeverity.Extra, LogCategory.UiNavigation);

        if (!DebugUtil.Check(minRange >= 0, $"Invalid range set, maxRange={maxRange}, minRange={minRange}"))
            minRange = 0;

        if (!DebugUtil.Check(maxRange >= minRange, $"Invalid range set, maxRange={maxRange}, minRange={minRange}"))
            maxRange = minRange;

        var key = (minRange, maxRange);
        if (_cachedPatterns.TryGetValue(key, out var cached))
            return cached;

        var pattern = ComputePattern(minRange, maxRange);
        _cachedPatterns[key] = pattern;

        return pattern;
    }

    /// <summary>
    /// Computes Manhattan offsets in rings for distances minRange..maxRange (diamond shape).
    /// </summary>
    private static Vector2I[] ComputePattern(int minRange, int maxRange)
    {
        Logger.Log("ComputePattern", LogSeverity.Extra, LogCategory.UiNavigation);

        var output = new List<Vector2I>();

        for (var distance = minRange; distance <= maxRange; distance++)
        {
            for (var dx = -distance; dx <= distance; dx++)
            {
                var dyMagnitude = distance - System.Math.Abs(dx);

                output.Add(new Vector2I(dx, dyMagnitude));
                if (dyMagnitude != 0)
                    output.Add(new Vector2I(dx, -dyMagnitude));
            }
        }

        return output.ToArray();
    }
}