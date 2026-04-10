#nullable enable
using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

/// <summary>
/// Provides cached Manhattan-range offset patterns and helpers to apply them to origin cells.
/// This is action-agnostic (Attack/Ability/etc. decide what these cells mean).
/// </summary>
public sealed class ManhattanRangeService
{
    private static readonly GobLogger Logger = GobLogManager.For<ManhattanRangeService>();

    private static readonly Dictionary<(int MinRange, int MaxRange), Vector2I[]> CachedPatterns = new();

    public static Vector2I[] GetPattern(int minRange, int maxRange)
    {
        Logger.Log("GetPattern", GobLogSeverity.Extra, GobLogCategory.UiNavigation);

        if (!DebugUtil.Check(minRange >= 0, $"Invalid range set, maxRange={maxRange}, minRange={minRange}"))
            minRange = 0;

        if (!DebugUtil.Check(maxRange >= minRange, $"Invalid range set, maxRange={maxRange}, minRange={minRange}"))
            maxRange = minRange;

        var key = (minRange, maxRange);
        if (CachedPatterns.TryGetValue(key, out var cached))
            return cached;

        var pattern = ComputePattern(minRange, maxRange);
        CachedPatterns[key] = pattern;

        return pattern;
    }

    /// <summary>
    /// Computes Manhattan offsets in rings for distances minRange..maxRange (diamond shape).
    /// </summary>
    private static Vector2I[] ComputePattern(int minRange, int maxRange)
    {
        Logger.Log("ComputePattern", GobLogSeverity.Extra, GobLogCategory.UiNavigation);

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

    public static int GetDistance(Vector2I origin, Vector2I target)
    {
        var delta = origin - target;
        return Math.Abs(delta.X) + Math.Abs(delta.Y);
    }

    public static int GetDistance(Vector2 origin, Vector2 target) =>
        GetDistance(GridNavigationUtil.ToCell(origin), GridNavigationUtil.ToCell(target));
}