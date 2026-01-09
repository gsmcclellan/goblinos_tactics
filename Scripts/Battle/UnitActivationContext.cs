#nullable enable
using Godot;

namespace Goblinos.Scripts.Battle;

public sealed class UnitActivationContext
{
    public BattleUnit Unit { get; }
    public Vector2I OriginCell { get; }

    /// <summary>
    /// Move destination. Null means has not moved.
    /// </summary>
    public Vector2I? MoveTargetCell { get; private set; }

    /// <summary>
    /// The primary action such as attack, null means has not acted.
    /// </summary>
    public PrimaryActionType PrimaryActionType { get; private set; } = PrimaryActionType.None;

    /// <summary>
    /// Optional target cell for the primary action.
    /// Required for Attack / Spell / Ability.
    /// </summary>
    public Vector2I? PrimaryActionTargetCell { get; private set; }
    public BattleUnit? PrimaryActionTargetUnit { get; private set; }

    public bool HasPlannedMove => MoveTargetCell.HasValue;
    public bool HasPlannedPrimaryAction => PrimaryActionType != PrimaryActionType.None;
    public bool HasRequiredTarget =>
        !RequiresTarget || PrimaryActionTargetCell.HasValue;
    public bool RequiresTarget =>
        PrimaryActionType is PrimaryActionType.Attack or PrimaryActionType.Spell or PrimaryActionType.Ability;

    
    public UnitActivationContext(BattleUnit unit, Vector2I originCell)
    {
        Unit = unit;
        OriginCell = originCell;
    }

    public void SetMoveTargetCell(Vector2I targetCell)
    {
        MoveTargetCell = targetCell;
    }

    public void ClearMoveTargetCell()
    {
        MoveTargetCell = null;
    }

    public void SetPrimaryAction(PrimaryActionType actionType, Vector2I? targetCell, BattleUnit? targetUnit)
    {
        PrimaryActionType = actionType;
        PrimaryActionTargetCell = targetCell;
        PrimaryActionTargetUnit = targetUnit;
    }

    public void ClearPrimaryAction()
    {
        PrimaryActionType = PrimaryActionType.None;
        PrimaryActionTargetCell = null;
        PrimaryActionTargetUnit = null;
    }

    public void Reset()
    {
        ClearMoveTargetCell();
        ClearPrimaryAction();
    }
}