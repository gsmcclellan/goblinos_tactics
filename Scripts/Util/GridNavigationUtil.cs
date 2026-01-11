#nullable enable
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

/// <summary>
/// Shared grid navigation helpers to keep neighbor ordering consistent across the game.
/// This ordering acts as a global tie-breaker for equal-cost paths.
/// </summary>
public static class GridNavigationUtil
{


    /// <summary>
    /// Canonical cardinal direction order used across the project.
    /// Pick one and never change it unless you're OK with path "feel" changing everywhere.
    /// </summary>
    public static readonly Vector2I[] CardinalDirections = InputUtil.DirectionVectors;

    /// <summary>
    /// Enumerates the four neighbors of a cell in canonical order.
    /// </summary>
    public static IEnumerable<Vector2I> GetCardinalNeighbors(Vector2I cell)
    {
        LogManager.Log("[GridNavigation] GetCardinalNeighbors", LogSeverity.Trace, LogCategory.DebugOnly);

        for (var i = 0; i < CardinalDirections.Length; i++)
            yield return cell + CardinalDirections[i];
    }
}