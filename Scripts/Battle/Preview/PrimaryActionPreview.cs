using System.Collections.Generic;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Combat.Types;
using Godot;

namespace Goblinos.Scripts.Battle.Preview;

public class PrimaryActionPreview
{
    public PrimaryActionType ActionType { get; }
    public RangeBand Range { get; }
    public Vector2I OriginCell { get; }
    
    /// <summary>
    /// All origin cells from which this action may be executed in the current activation.
    /// For "no-move targeting", this will be a single cell (the unit's current position).
    /// </summary>
    public IReadOnlySet<Vector2I> AttackOriginCells { get; }
    /// <summary>
    /// All target cells that are legal for this action from at least one origin.
    /// </summary>
    public IReadOnlySet<Vector2I> TargetCells { get; }
    /// <summary>
    /// For each target cell, the set of origin cells from which it can be targeted.
    /// </summary>
    public IReadOnlyDictionary<Vector2I, IReadOnlySet<Vector2I>> AttackOriginsByTargetCell { get; }
    /// <summary>
    /// For each attack origin cell (or move target cell), the set of primary action target cells reachable from that origin.
    /// </summary>
    public IReadOnlyDictionary<Vector2I, IReadOnlySet<Vector2I>> TargetCellsByAttackOrigin  { get; }

    public PrimaryActionPreview(
        PrimaryActionType actionType,
        RangeBand range,
        Vector2I originCell,
        IReadOnlySet<Vector2I> attackOriginCells,
        IReadOnlySet<Vector2I> targetCells,
        IReadOnlyDictionary<Vector2I, IReadOnlySet<Vector2I>> attackOriginsByTargetCell,
        IReadOnlyDictionary<Vector2I, IReadOnlySet<Vector2I>> targetCellsByAttackOrigin)
    {
        ActionType = actionType;
        Range = range;
        OriginCell = originCell;
        AttackOriginCells = attackOriginCells;
        TargetCells = targetCells;
        AttackOriginsByTargetCell = attackOriginsByTargetCell;
        TargetCellsByAttackOrigin = targetCellsByAttackOrigin;
    }
    
    public bool CanTargetCell(Vector2I targetCell)
    {
        return TargetCells.Contains(targetCell);
    }

    
    public bool GetAttackOrigins(Vector2I targetCell, out IReadOnlySet<Vector2I> origins)
    {
        return AttackOriginsByTargetCell.TryGetValue(targetCell, out origins);
    }

    public bool TryGetTargetCells(Vector2I originCell, out IReadOnlySet<Vector2I> targets)
    {
        return TargetCellsByAttackOrigin.TryGetValue(originCell, out targets);
    }
}