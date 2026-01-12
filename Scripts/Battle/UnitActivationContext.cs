#nullable enable
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Godot;

namespace Goblinos.Scripts.Battle;

// TODO - investigate. Undo maybe not needed on primary action because it is not resolved until confirmation which then 
// can't be undone.
public sealed class UnitActivationContext
{
    private readonly Logger _logger = LogManager.For<UnitActivationContext>();
    public BattleUnit Unit { get; }
    public Vector2I OriginCell { get; }

    /// <summary>
    /// Move destination. Null means has not moved.
    /// </summary>
    public Vector2I? MoveTargetCell { get; private set; }

    /// <summary>
    /// The primary action such as attack, null means has not acted.
    /// </summary>
    public PrimaryActionType PrimaryAction { get; private set; } = PrimaryActionType.None;

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
    public bool CanUndoMove => HasMoved && !BlockUndoMove;
    /// <summary>Primary action queued, undo is not blocked</summary>
    public bool CanUndoPrimaryAction => HasSelectedPrimaryAction && !BlockUndoPrimaryAction;
    public bool HasMoved => MoveTargetCell.HasValue;
    public bool HasSelectedPrimaryAction => PrimaryAction != PrimaryActionType.None;
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
        _logger.Log($"Created - Unit={unit}, OriginCell={originCell}", LogSeverity.Trace, LogCategory.Initialization);
        Unit = unit;
        OriginCell = originCell;
    }

    public void SetMoveTargetCell(Vector2I targetCell)
    {
        _logger.Log($"SetMoveTargetCell - target={targetCell}", LogSeverity.Trace, LogCategory.UnitLifecycle);
        MoveTargetCell = targetCell;
    }

    public void ClearMoveTargetCell()
    {
        _logger.Log("ClearMoveTargetCell", LogSeverity.Trace, LogCategory.UnitLifecycle);
        MoveTargetCell = null;
    }

    public void SetPrimaryAction(PrimaryActionType action)
    {
        _logger.Log($"SetPrimaryAction - action={action}", LogSeverity.Trace, LogCategory.UnitLifecycle);
        PrimaryAction = action;
    }

    public void SetPrimaryActionTarget(Vector2I cell, BattleUnit unit)
    {
        _logger.Log($"SetPrimaryActionTarget - cell={cell}, unit={unit}", LogSeverity.Trace, LogCategory.UnitLifecycle);
        PrimaryActionTargetCell = cell;
        PrimaryActionTargetUnit = unit;
    }

    public void ClearPrimaryAction()
    {
        _logger.Log("ClearPrimaryAction", LogSeverity.Trace, LogCategory.UnitLifecycle);
        PrimaryAction = PrimaryActionType.None;
        PrimaryActionTargetCell = null;
        PrimaryActionTargetUnit = null;
    }

    public void Reset()
    {
        _logger.Log("Reset", LogSeverity.Trace, LogCategory.UnitLifecycle);
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