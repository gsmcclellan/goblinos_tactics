using System;
using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

public sealed class MoveRangeService
{
    private readonly Logger _logger = LogManager.For<MoveRangeService>();
    
    private readonly Core.BattleGrid _grid;
    private readonly Units.UnitRegistry _unitRegistry;

    private int _gridRevision;

    private readonly Dictionary<(Vector2I Cell, string UnitId, int GridRevision), MovementPreview> _cache =
        new();

    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    public MoveRangeService(Core.BattleGrid grid, Units.UnitRegistry unitRegistry)
    {
        _grid = grid;
        _unitRegistry = unitRegistry;
    }
    
    // ---------------------------------------------------------------------
    // Public Methods
    // ---------------------------------------------------------------------

    public bool CanMoveTo(Units.BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        _logger.Log($"CanMoveTo unit={unit.Name} from={fromCell} to={toCell}", LogSeverity.Trace, LogCategory.UiNavigation);
        
        // Destination must exist and be walkable.
        if (!_grid.TryGetTerrainAtCell(toCell, out var destinationTerrain))
            return false;
        
        if (destinationTerrain.BlocksMovement)
            return false;

        // Do not allow ending movement on another unit.
        // (Adjust this if you want to allow ending on allies for swap/stack/etc.)
        if (_unitRegistry.TryGetUnitAtCell(toCell, out var occupyingUnit) && occupyingUnit != unit)
            return false;
        
        // Use the cached movement preview to answer reachability.
        if (unit.Movement <= 0)
            return false;
        
        var preview = GetMovementPreview(fromCell, unit);
        return preview.Cells.Contains(toCell);
    }
    
    /// <summary>
    /// Returns a cached movement preview when available, otherwise computes and caches it.
    /// </summary>
    public MovementPreview GetMovementPreview(Vector2I startCell, Units.BattleUnit actingUnit)
    {
        var cacheKey = (startCell, actingUnit.Id, _gridRevision);
        if (_cache.TryGetValue(cacheKey, out var movePreview))
            return movePreview;
        movePreview = BuildMovementPreview(startCell, actingUnit);
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
    /// TODO - add function that takes multiple starting cells (for enemy threat range)
    private MovementPreview BuildMovementPreview(Vector2I startCell, Units.BattleUnit actingUnit)
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
            if (costSoFar > actingUnit.Movement)
                continue;
            
            foreach (var neighbor in GridNavigationUtil.GetCardinalNeighbors(cell))
            {
                // skip if no terrain or terrain blocks movement
                if (!_grid.TryGetTerrainAtCell(neighbor, out var terrain) || terrain.BlocksMovement)
                    continue;
                
                // Skip if enemy unit.
                if (_unitRegistry.TryGetUnitAtCell(neighbor, out var existingUnit) &&
                    existingUnit.IsFriendly != actingUnit.IsFriendly)
                    continue;
                
                // add step cost to costSoFar, if less than movePoints
                var addCost = Math.Max(1, terrain.MoveCost);
                var newCost = costSoFar + addCost;

                if (newCost > actingUnit.Movement)
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
        
        return new MovementPreview()
        {
            Cells = new HashSet<Vector2I>(bestCost.Keys),
            CostByCell = bestCost,
            ParentCells = parentCells,
            OriginCell = startCell
        };
    }
}