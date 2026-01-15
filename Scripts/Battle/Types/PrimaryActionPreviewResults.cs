using System.Collections.Generic;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle.Types;

public class PrimaryActionPreviewResults
{
    private readonly Dictionary<PrimaryActionType, IReadOnlySet<Vector2I>> _targetsByOption;
    
    /// <summary>
    /// Creates an empty previews container.
    /// </summary>
    public PrimaryActionPreviewResults()
    {
        _targetsByOption = new Dictionary<PrimaryActionType, IReadOnlySet<Vector2I>>();
    }
    
    /// <summary>
    /// Clears all stored previews.
    /// </summary>
    public void Clear()
    {
        _targetsByOption.Clear();
    }

    /// <summary>
    /// Sets the valid target cells for a given option (replaces any existing set).
    /// </summary>
    public void SetTargetPreview(PrimaryActionType option, IEnumerable<Vector2I> targetCells)
    {
        if (!DebugUtil.Require(targetCells != null, "SetTargets received null targetCells."))
            return;

        var set = new HashSet<Vector2I>();
        foreach (var cell in targetCells)
            set.Add(cell);

        _targetsByOption[option] = set;
    }
    
    /// <summary>
    /// Returns true if the option has any valid targets.
    /// </summary>
    public bool HasTargets(PrimaryActionType option)
    {
        return _targetsByOption.TryGetValue(option, out var set) && set.Count > 0;
    }
    
    /// <summary>
    /// Returns true if the specified cell is a valid target for the given option.
    /// </summary>
    public bool IsValidTarget(PrimaryActionType option, Vector2I cell)
    {
        return _targetsByOption.TryGetValue(option, out var set) && set.Contains(cell);
    }
    
    /// <summary>
    /// Returns a union of all target cells across options (useful for "any targetable" overlays).
    /// </summary>
    public IReadOnlySet<Vector2I> GetAllTargetsUnion()
    {
        var union = new HashSet<Vector2I>();
        foreach (var kvp in _targetsByOption)
            union.UnionWith(kvp.Value);

        return union;
    }
    
    /// <summary>
    /// Returns a copy of the targets as a HashSet. Returns an empty set if none exist.
    /// </summary>
    public IReadOnlySet<Vector2I> GetTargets(PrimaryActionType option)
    {
        return _targetsByOption.TryGetValue(option, out var set)
            ? set
            : new HashSet<Vector2I>();
    }
}