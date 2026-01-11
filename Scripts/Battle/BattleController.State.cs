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

    private void AbortActivationToFreeSelect()
    {
        _logger.Log("AbortActivationToFreeSelect", LogSeverity.Info, LogCategory.UnitLifecycle);
        // Cancel unit activation, go back to free select.
        if (!TryUndoMove())
            throw new Exception("Try undo move failed, can't reset.");
        
        ExitTargetingMode();
        // TODO - update cursor
        // add enum for different beaviors AbortBehavior { KeepCursor, RecenterOnOrigin }
        ClearUnitActivation();
        ResetActivationPreview();
    }

    private void ClearActivationPreviews()
    {
        _grid.ClearOverlays();
    }
    
    private void ClearUnitActivation()
    {
        // Currently does not account for actions that could prevent undo. Change this if adding traps, reactions etc.

        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.State] ResetUnitActivation failed. No UnitActivationContext."))
            return;
        
        _unitActivation.Reset();
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

    private void GenerateMovementPreviewForHoveredCell()
    {
        
        var cell = _selectionController.HoveredCell;
        var isHoveredUnit = _unitRegistry.TryGetUnitAtCell(cell, out var regUnit);
        var unit = _selectionController.HoveredUnit;
        _logger.Log($"GenerateMovementPreviewForHoveredCell cell={cell} unit={unit?.UnitName} regUnit={regUnit?.UnitName}", LogSeverity.Info, LogCategory.UiNavigation);
        if (unit == null)
            return;
        
        var movementPreview = _moveRangeService.BuildMovementPreview(cell, unit.Movement);
        _unitActivation = new UnitActivationContext(unit, cell);
        _grid.SetMovementPreview(movementPreview);
    }
    
    private void ResetActivationPreview()
    {
        ClearActivationPreviews();
        GenerateMovementPreviewForHoveredCell();
        // TODO - add attack preview
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

    private bool TryUndoMove()
    {
        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.Input].TryUndoMove - Unable to undo move action, no UnitActivationContext"))
            return false;

        if (!_unitActivation.HasMoved)
            return true;
        if (!_unitActivation.CanUndoMove)
            return false;
        if (!_movementController.TryMoveToCell(_unitActivation.Unit, _unitActivation.OriginCell))
            return false;

        _unitActivation.ClearMoveTargetCell();
        _selectionController.UpdateHovered();
        return true;
    }

    private bool TryClearUnitActivation()
    {
        if (DebugUtil.Require(_unitActivation != null, "Unable to undo move action, no UnitActivationContext"))
            return false;
        
        if (!_unitActivation.CanReset)
            return false;

        ClearUnitActivation();
        return true;
    }
}