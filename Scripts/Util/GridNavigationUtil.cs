#nullable enable
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Core;
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
    
    public static Vector2I ToCell(Vector2 world)
    {
        // If TileSize is an int, this is fine. If it's Vector2, adjust accordingly.
        var cellX = Mathf.FloorToInt(world.X / GlobalSettings.TileSize);
        var cellY = Mathf.FloorToInt(world.Y / GlobalSettings.TileSize);

        return new Vector2I(cellX, cellY);
    }
}