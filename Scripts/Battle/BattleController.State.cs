#nullable enable
using System;
using Goblinos.Logging;
using Goblinos.Scripts.Util;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Fields */
    private BattleInputState _inputState = BattleInputState.FreeSelect;
    private UnitActivationContext? _unitActivation;
    
    /** Properties */
    public BattleInputState InputState
    {
        get => _inputState;
        private set
        {
            if (value == _inputState) return; 
            _inputState = value;
            NotifyInputStateChanged(value);
        }
    }

    private void _Ready_State()
    {
        
    }

    private void SetInputState(BattleInputState state, BattleUnit? unit)
    {
        switch (state)
        {
            case BattleInputState.FreeSelect:
                ExitTargetingMode();
                break;
            case BattleInputState.MoveTargeting:
                if (DebugUtil.Require(unit != null, "[BattleController.State].SetInputState Require unit to enter move state."))
                    return;
                EnterMoveTargetingMode(unit);
                break;
            case BattleInputState.PrimaryActionSelect:
                if (DebugUtil.Require(unit != null, "[BattleController.State].SetInputState Require unit to enter move state."))
                    return;
                EnterPrimaryActionSelectMode(unit);
                break;
            default:
                throw new NotImplementedException();
        }
    }
    
    private void EnterMoveTargetingMode(BattleUnit unit)
    {
        _logger.Log("EnterMovementMode", LogSeverity.Trace, LogCategory.Input);
        
        var cell = _selectionController.SelectedCell;
        if (!DebugUtil.Require(cell.HasValue, "[BattleController.Input] failed to enter MovementMode, no selected cell") ||
            !DebugUtil.Require(unit != null, "[BattleController.Input] failed to enter MovementMode, no unit")
           )
            return;
        
        InputState = BattleInputState.MoveTargeting;
        
        var movementPreview = _moveRangeService.BuildMovementPreview(cell.Value, unit.Movement);
        _unitActivation = new UnitActivationContext(unit, cell.Value);
        
        _grid.SetMovementPreview(movementPreview);
    }

    private void EnterPrimaryActionSelectMode(BattleUnit unit)
    {
        _logger.Log("EnterPrimaryActionSelectMode", LogSeverity.Trace, LogCategory.Input);
        
        var cell = _selectionController.SelectedCell;
        if (!DebugUtil.Require(cell.HasValue, "[BattleController.Input] failed to enter MovementMode, no selected cell") ||
            !DebugUtil.Require(unit != null, "[BattleController.Input] failed to enter MovementMode, no unit")
           )
            return;
        
        InputState = BattleInputState.PrimaryActionSelect;
        _grid.ClearOverlays();
        // TODO - show action menu
    }

    private void ExitPrimaryActionSelectMode()
    {
        _logger.Log("ExitTargetingMode", LogSeverity.Trace, LogCategory.Input);
        
        if (TryUndoMove())
            EnterMoveTargetingMode(_unitActivation.Unit);
    }
        
    private void ExitTargetingMode()
    {
        _logger.Log("ExitTargetingMode", LogSeverity.Trace, LogCategory.Input);

        InputState = BattleInputState.FreeSelect;
        _selectionController.TriggerClearSelection();
        _grid.ClearOverlays();
    }
    
    /// <summary>
    /// Resolved all pending actions, move + attack/ability/wait
    /// </summary>
    /// This can be refactored into own class - ActionResolver
    /// If many actions or becomes complex, refactor into one class per Action Type
    /// with shared interface containing TryExecute.
    private void ResolveUnitActions()
    {
        
    }

    private bool TryUndoMove()
    {
        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.Input].TryUndoMove - Unable to undo move action, no UnitActivationContext"))
            return false;

        if (!_unitActivation.HasPlannedMove)
            return true;
        if (!_unitActivation.CanUndoMove)
            return false;
        if (!_movementController.TryMoveToCell(_unitActivation.Unit, _unitActivation.OriginCell))
            return false;

        _unitActivation.ClearMoveTargetCell();
        return true;
    }

    private bool TryUndoUnitActivation()
    {
        if (DebugUtil.Require(_unitActivation != null, "Unable to undo move action, no UnitActivationContext"))
            return false;
        
        if (!_unitActivation.CanReset)
            return false;

        ResetUnitActivation();
        return true;
    }

    private void ResetUnitActivation()
    {
        // Currently does not account for actions that could prevent undo. Change this if adding traps, reactions etc.

        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.State] ResetUnitActivation failed. No UnitActivationContext."))
            return;
        
        _unitActivation.Reset();
    }

    private bool TryClearUnitActivationMove() // Future / unused. Maybe useful if actions block undo.
    {
        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.State] UndoMove failed. No UnitActivationContext."))
            return false;
        
        // If undo not required return true
        if (!_unitActivation.HasPlannedMove)
            return true;
        
        // If move exists but unable to undo, return false
        if (!_unitActivation.CanUndoMove)
            return false;

        // Clear the target
        _unitActivation.ClearMoveTargetCell();
        return true;
    }

    private bool TryClearUnitActivationPrimaryAction() // Future / unused. Maybe useful if actions block undo.
    {
        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.State] UndoPrimaryAction failed. No UnitActivationContext."))
            return false;
        
        // If undo not required return true
        if (!_unitActivation.HasPlannedPrimaryAction)
            return true;
        
        // If primary action exists but unable to undo, return false
        if (!_unitActivation.CanUndoPrimaryAction)
            return false;

        _unitActivation.ClearPrimaryAction();
        return true;
    }
}