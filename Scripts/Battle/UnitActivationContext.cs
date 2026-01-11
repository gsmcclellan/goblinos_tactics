#nullable enable
using Godot;

namespace Goblinos.Scripts.Battle;

// TODO - investigate. Undo maybe not needed on primary action because it is not resolved until confirmation which then 
// can't be undone.
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

    public bool BlockUndoMove; // set true if trap or reaction prevents undo
    public bool BlockUndoPrimaryAction; // set true after damage roll or something blocking undo

    public bool CanReset =>
        (!HasSelectedPrimaryAction || !BlockUndoPrimaryAction) && (!HasMoved || !BlockUndoMove);
    public bool CanUndo => (HasSelectedPrimaryAction && !BlockUndoPrimaryAction) || (!HasSelectedPrimaryAction && HasMoved && !BlockUndoMove);

    /// <summary>No primary action & undo move is not blocked</summary>
    public bool CanUndoMove => !HasSelectedPrimaryAction && HasMoved && !BlockUndoMove;
    /// <summary>Primary action queued, undo is not blocked</summary>
    public bool CanUndoPrimaryAction => HasSelectedPrimaryAction && !BlockUndoPrimaryAction;
    public bool HasMoved => MoveTargetCell.HasValue;
    public bool HasSelectedPrimaryAction => PrimaryActionType != PrimaryActionType.None;
    public bool HasRequiredTarget =>
        !RequiresTarget || PrimaryActionTargetCell.HasValue;
    public bool RequiresTarget =>
        PrimaryActionType is PrimaryActionType.Attack or PrimaryActionType.Spell or PrimaryActionType.Ability;
    public UnitActivationPhase UndoTarget {
        get
        {
            if (HasSelectedPrimaryAction)
                return UnitActivationPhase.PrimaryAction;
            if (HasMoved)
                return UnitActivationPhase.Movement;
            return UnitActivationPhase.None;
        }
    }

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

public enum UnitActivationPhase
{
    None,
    Movement,
    PrimaryAction
}