#nullable enable
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Godot;

namespace Goblinos.Scripts.Battle.Preview;

// TODO - investigate. Undo maybe not needed on primary action because it is not resolved until confirmation which then 
// can't be undone.
public sealed class UnitActivationContext: IUnitActionPlan
{
    private readonly GobLogger _logger = GobLogManager.For<UnitActivationContext>();
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
    public bool HasCombatPreview => PrimaryAction == PrimaryActionType.Attack && PrimaryActionTargetUnit != null;
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
        _logger.Log($"Created - Unit={unit}, OriginCell={originCell}", GobLogSeverity.Trace, GobLogCategory.Initialization);
        Unit = unit;
        OriginCell = originCell;
    }

    public void SetMoveTargetCell(Vector2I targetCell)
    {
        _logger.Log($"SetMoveTargetCell - target={targetCell}", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
        MoveTargetCell = targetCell;
    }

    public void ClearMoveTargetCell()
    {
        _logger.Log("ClearMoveTargetCell", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
        MoveTargetCell = null;
    }

    public void SetPrimaryAction(PrimaryActionType action)
    {
        _logger.Log($"SetPrimaryAction - action={action}", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
        PrimaryAction = action;
    }

    public void SetPrimaryActionTarget(Vector2I cell, BattleUnit unit)
    {
        _logger.Log($"SetPrimaryActionTarget - cell={cell}, unit={unit}", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
        PrimaryActionTargetCell = cell;
        PrimaryActionTargetUnit = unit;
    }

    public void ClearPrimaryAction()
    {
        _logger.Log("ClearPrimaryAction", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
        PrimaryAction = PrimaryActionType.None;
        PrimaryActionTargetCell = null;
        PrimaryActionTargetUnit = null;
    }

    public void Reset()
    {
        _logger.Log("Reset", GobLogSeverity.Trace, GobLogCategory.UnitLifecycle);
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