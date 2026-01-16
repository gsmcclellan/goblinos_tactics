using System.Collections.Generic;
using Goblinos.Logging;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Services;

public sealed class TargetRangeService
{
    private readonly Logger _logger = LogManager.For<TargetRangeService>();

    private readonly BattleGrid _grid;

    public TargetRangeService(BattleGrid grid)
    {
        _grid = grid;
    }
    
    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells, adding to existing set.
    /// Useful for enemy threat previews and "move+attack" threat union.
    /// For multiple enemies with different ranges, call once per enemy and union results.
    /// </summary>
    public void AddThreatUnionFromCells(IEnumerable<Vector2I> originCells, int minRange, int maxRange, HashSet<Vector2I> output)
    {
        var offsets = GetTargetPattern(minRange, maxRange);

        foreach (var originCell in originCells)
        {
            for (var i = 0; i < offsets.Length; i++)
                AddIfValidCell(originCell + offsets[i], output);
        }
    }

    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells, adding to existing set.
    /// Useful for enemy threat previews and "move+attack" threat union.
    /// For multiple enemies with different ranges, call once per enemy and union results.
    /// </summary>
    public void AddThreatUnionFromCells(IEnumerable<Vector2I> originCells, RangeBand range,
        HashSet<Vector2I> output) => AddThreatUnionFromCells(originCells, range.Min, range.Max, output);

    
    /// <summary>
    /// Builds the set of target cells that are attackable from a single origin cell,
    /// using Manhattan distance (diamond shape).
    /// </summary>
    public HashSet<Vector2I> BuildTargetRangeFromCell(Vector2I originCell, int minRange, int maxRange)
    {
        var results = new HashSet<Vector2I>();
        AddInRangeCellsFromOrigin(originCell, minRange, maxRange, results);

        return results;
    }
    
    /// <summary>
    /// Builds the set of target cells that are attackable from a single origin cell,
    /// using Manhattan distance (diamond shape).
    /// </summary>
    public HashSet<Vector2I> BuildTargetRangeFromCell(Vector2I originCell, RangeBand range) => BuildTargetRangeFromCell(originCell, range.Min, range.Max);
    
    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells.
    /// For multiple enemies with different ranges, use AddAttackThreatUnionFromCells per enemy and union results.
    /// </summary>
    public HashSet<Vector2I> BuildThreatUnionFromCells(IEnumerable<Vector2I> originCells, int minRange, int maxRange)
    {
        var results = new HashSet<Vector2I>();
        AddThreatUnionFromCells(originCells, minRange, maxRange, results);
        _logger.Log($"BuildAttackThreatUnionFromCells - results.Count={results.Count}", LogSeverity.Extra, LogCategory.UiNavigation);
        return results;
    }
    
    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells.
    /// For multiple enemies with different ranges, use AddAttackThreatUnionFromCells per enemy and union results.
    /// </summary>
    public HashSet<Vector2I> BuildThreatUnionFromCells(IEnumerable<Vector2I> originCells, RangeBand range) => BuildThreatUnionFromCells(originCells, range.Min, range.Max);
    
    /// <summary>
    /// Adds all cells in a Manhattan ring between minRange and maxRange to the output set.
    /// Filters out cells that are not valid terrain cells.
    /// </summary>
    private void AddInRangeCellsFromOrigin(Vector2I originCell, int minRange, int maxRange, HashSet<Vector2I> output)
    {
        var offsets = GetTargetPattern(minRange, maxRange);
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < offsets.Length; i++)
            AddIfValidCell(originCell + offsets[i], output);

        // Optional: If you never want to include the origin cell even when minRange == 0.
        // output.Add(originCell);
    }
    
    private void AddIfValidCell(Vector2I cell, HashSet<Vector2I> output)
    {
        // TODO - filter out cells that can never be occupied.
        // Treat "exists on grid" as "has terrain".
        if (!_grid.TryGetTerrainAtCell(cell, out _ /*var terrain*/) /*|| terrain.BlocksOccupancy*/) 
            return;

        output.Add(cell);
    }

    private Vector2I[] GetTargetPattern(int minRange, int maxRange) => 
        ManhattanRangeService.GetPattern(minRange, maxRange);
}