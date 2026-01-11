using System.Collections.Generic;
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public sealed class AttackRangeService
{
    private readonly Logger _logger = LogManager.For<AttackRangeService>();

    private readonly BattleGrid _grid;

    public AttackRangeService(BattleGrid grid)
    {
        _grid = grid;
    }

    private readonly Dictionary<(int MinRange, int MaxRange), Vector2I[]> _cachedAttackPatterns = new();

    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells, adding to existing set.
    /// Useful for enemy threat previews and "move+attack" threat union.
    /// For multiple enemies with different ranges, call once per enemy and union results.
    /// </summary>
    public void AddAttackThreatUnionFromCells(IEnumerable<Vector2I> originCells, int minRange, int maxRange, HashSet<Vector2I> output)
    {
        var offsets = GetAttackPattern(minRange, maxRange);

        var count = 0;
        foreach (var originCell in originCells)
        {
            GD.Print($"originCell: {count}");
            count++;
            for (var i = 0; i < offsets.Length; i++)
                AddIfValidAttackableCell(originCell + offsets[i], output);
        }
    }

    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells, adding to existing set.
    /// Useful for enemy threat previews and "move+attack" threat union.
    /// For multiple enemies with different ranges, call once per enemy and union results.
    /// </summary>
    public void AddAttackThreatUnionFromCells(IEnumerable<Vector2I> originCells, RangeBand range,
        HashSet<Vector2I> output) => AddAttackThreatUnionFromCells(originCells, range.Min, range.Max, output);

    
    /// <summary>
    /// Builds the set of target cells that are attackable from a single origin cell,
    /// using Manhattan distance (diamond shape).
    /// </summary>
    public HashSet<Vector2I> BuildAttackRangeFromCell(Vector2I originCell, int minRange, int maxRange)
    {
        var results = new HashSet<Vector2I>();
        AddAttackCellsFromOrigin(originCell, minRange, maxRange, results);

        return results;
    }
    
    /// <summary>
    /// Builds the set of target cells that are attackable from a single origin cell,
    /// using Manhattan distance (diamond shape).
    /// </summary>
    public HashSet<Vector2I> BuildAttackRangeFromCell(Vector2I originCell, RangeBand range) => BuildAttackRangeFromCell(originCell, range.Min, range.Max);
    
    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells.
    /// For multiple enemies with different ranges, use AddAttackThreatUnionFromCells per enemy and union results.
    /// </summary>
    public HashSet<Vector2I> BuildAttackThreatUnionFromCells(IEnumerable<Vector2I> originCells, int minRange, int maxRange)
    {
        var results = new HashSet<Vector2I>();
        AddAttackThreatUnionFromCells(originCells, minRange, maxRange, results);
        _logger.Log($"BuildAttackThreatUnionFromCells - results.Count={results.Count}", LogSeverity.Info, LogCategory.UiNavigation);
        return results;
    }
    
    /// <summary>
    /// Builds the union of all attackable cells from a set of origin cells.
    /// For multiple enemies with different ranges, use AddAttackThreatUnionFromCells per enemy and union results.
    /// </summary>
    public HashSet<Vector2I> BuildAttackThreatUnionFromCells(IEnumerable<Vector2I> originCells, RangeBand range) => BuildAttackThreatUnionFromCells(originCells, range.Min, range.Max);
    
    /// <summary>
    /// Adds all cells in a Manhattan ring between minRange and maxRange to the output set.
    /// Filters out cells that are not valid terrain cells.
    /// </summary>
    private void AddAttackCellsFromOrigin(Vector2I originCell, int minRange, int maxRange, HashSet<Vector2I> output)
    {
        var offsets = GetAttackPattern(minRange, maxRange);
        
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var i = 0; i < offsets.Length; i++)
            AddIfValidAttackableCell(originCell + offsets[i], output);

        // Optional: If you never want to include the origin cell even when minRange == 0.
        // output.Add(originCell);
    }
    
    private void AddIfValidAttackableCell(Vector2I cell, HashSet<Vector2I> output)
    {
        // TODO - filter out cells that can never be occupied.
        // Treat "exists on grid" as "has terrain".
        if (!_grid.TryGetTerrainAtCell(cell, out _ /*var terrain*/) /*|| terrain.BlocksOccupancy*/) 
            return;

        output.Add(cell);
    }
    
    private void CacheAttackPattern(int minRange, int maxRange, Vector2I[] attackPattern)
    {
        _cachedAttackPatterns[(minRange, maxRange)] = attackPattern;
    }

    /// <summary>
    /// Computes a pattern of legal attacks, these offsets can be added to position to produce attack targets.
    /// Example: range of 1 produces a 4 target diamond pattern -> up, right, down, left
    ///          range of 2 produces an 8 target diamond
    ///          range of 1-2 produces the sum of these two.
    /// </summary>
    private Vector2I[] ComputeAttackPattern(int minRange, int maxRange)
    {
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

        _logger.Log($"ComputeAttackPattern - output.Count={output.Count}, minRange={minRange}, maxRange={maxRange}", LogSeverity.Extra, LogCategory.UiNavigation);
        return output.ToArray();
    }

    private Vector2I[] GetAttackPattern(int minRange, int maxRange)
    {
        if (minRange < 0)
            minRange = 0;

        if (!DebugUtil.Check(maxRange >= minRange, $"Invalid range set, maxRange={maxRange}, minRange={minRange}"))
            maxRange = minRange;

        var key = (minRange, maxRange);
        if (_cachedAttackPatterns.TryGetValue(key, out var cachedAttackPattern))
            return cachedAttackPattern;

        var attackPattern = ComputeAttackPattern(minRange, maxRange);
        CacheAttackPattern(minRange, maxRange, attackPattern);
        return attackPattern;
    }
}