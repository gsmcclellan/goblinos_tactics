using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public sealed class MoveRangeService
{
    private readonly Logger _logger = LogManager.For<MoveRangeService>();
    
    private readonly BattleGrid _grid;

    private int _gridRevision;

    private readonly Dictionary<(Vector2I Cell, int MovePoints, int GridRevision), MovementPreviewResults> _cache =
        new();

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public MoveRangeService(BattleGrid grid)
    {
        _grid = grid;
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    /// <summary>
    /// Returns a cached movement preview when available, otherwise computes and caches it.
    /// </summary>
    public MovementPreviewResults GetMovementPreview(Vector2I startCell, int movePoints)
    {
        var cacheKey = (startCell, movePoints, _gridRevision);
        if (_cache.TryGetValue(cacheKey, out var movePreview))
            return movePreview;
        movePreview = BuildMovementPreview(startCell, movePoints);
        _cache[cacheKey] = movePreview;
        return movePreview;
    }

    public void InvalidateCache()
    {
        _gridRevision++;
        _logger.Log($"InvalidateCache rev={_gridRevision}", LogSeverity.Info, LogCategory.UnitLifecycle);
    }
    
    public static List<Vector2I> ReconstructPath(IReadOnlyDictionary<Vector2I, Vector2I> parentCells, Vector2I startCell,
        Vector2I targetCell, List<Vector2I> outputPath)
    {
        outputPath.Clear();

        if (targetCell == startCell)
        {
            outputPath.Add(startCell);
            return outputPath;
        }
        
        var current = targetCell;
        outputPath.Add(current);

        while (current != startCell && outputPath.Count < 4096)
        {
            if (!DebugUtil.Require(parentCells.TryGetValue(current, out var parent),
                    $"Broken path, missing parent of {current}"))
            {
                outputPath.Clear();
                return outputPath;
            }
            
            current = parent;
            outputPath.Add(current);
        }
        
        if (!DebugUtil.Require(current == startCell, "[MoveRangeService] ReconstructPath - Something went wrong, did not reach start cell"))
        {
            outputPath.Clear();
            return outputPath;
        }

        outputPath.Reverse();
        LogManager.LogInternal(typeof(MoveRangeService), nameof(MoveRangeService), $"ReconstructPath :: {string.Join(" -> ", outputPath)}", LogSeverity.Extra, LogCategory.UiNavigation);
        LogManager.LogInternal(typeof(MoveRangeService), nameof(MoveRangeService), $"ReconstructPath :: pathLen={outputPath.Count}", LogSeverity.Extra, LogCategory.UiNavigation);
        return outputPath;
    }
    
    
    // ---------------------------------------------------------------------
    // Private Methods
    // ---------------------------------------------------------------------

    /// <summary>
    /// Computes all reachable cells within the given movement budget using Dijkstra.
    /// </summary>
    /// TODO - block movement through enemy units.
    /// TODO - add function that takes multiple starting cells (for enemy threat range)
    private MovementPreviewResults BuildMovementPreview(Vector2I startCell, int movePoints)
    {
        _logger.Log("GetReachableCells", LogSeverity.Trace, LogCategory.UiNavigation);

        var bestCost = new Dictionary<Vector2I, int>();
        var parentCells = new Dictionary<Vector2I, Vector2I>();

        var open = new PriorityQueue<(Vector2I Cell, int Cost), int>();
        bestCost[startCell] = 0;
        open.Enqueue((startCell, 0), 0);

        while (open.Count > 0)
        {
            var (cell, costSoFar) = open.Dequeue();
            if (!bestCost.TryGetValue(cell, out var resolvedCost) || costSoFar != resolvedCost)
                continue; // stale entry, already resolved better cost for this cell
            
            // Skip nodes beyond budget.
            if (costSoFar > movePoints)
                continue;
            
            foreach (var neighbor in GridNavigationUtil.GetCardinalNeighbors(cell))
            {
                // skip if no terrain or terrain blocks movement
                if (!_grid.TryGetTerrainAtCell(neighbor, out var terrain) || terrain.BlocksMovement)
                    continue;
                
                // add step cost to costSoFar, if less than movePoints
                var addCost = Math.Max(1, terrain.MoveCost);
                var newCost = costSoFar + addCost;

                if (newCost > movePoints)
                    continue;

                // skip if already lower or equal cost
                if (bestCost.TryGetValue(neighbor, out var existingCost) && existingCost <= newCost)
                    continue;
                
                bestCost[neighbor] = newCost;
                parentCells[neighbor] = cell;
                open.Enqueue((neighbor, newCost), newCost);
            }
        }
        
        _logger.Log($"GetReachableCells Count={bestCost.Count}", LogSeverity.Trace, LogCategory.UiNavigation);
        
        return new MovementPreviewResults()
        {
            Cells = new HashSet<Vector2I>(bestCost.Keys),
            CostByCell = bestCost,
            ParentCells = parentCells,
            StartCell = startCell
        };
    }
}

public sealed class MovementPreviewResults
{
    public required IReadOnlySet<Vector2I> Cells { get; init; }
    public required IReadOnlyDictionary<Vector2I, int> CostByCell { get; init; }
    public required IReadOnlyDictionary<Vector2I, Vector2I> ParentCells { get; init; }
    public required Vector2I StartCell { get; init; }
}