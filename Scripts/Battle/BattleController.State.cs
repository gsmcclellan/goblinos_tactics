#nullable enable
using System;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
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

    public bool IsUnitSelected => _selectionController.IsUnitSelected;

    private void _Ready_State()
    {
        Debug.Assert(_unitRegistry != null, "[BattleController.Units] Not Initialized. Unable to register UnitRegistry.");
        _registerExistingBattleUnitNodes();
        
        _logger.Log("_Ready_State", LogSeverity.Info, LogCategory.Initialization);
    }
    
    /// <summary>
    /// Test function - registers Units in node path. Will be replaced with loading from UnitData.
    /// </summary>
    private void _registerExistingBattleUnitNodes()
    {
        var units = _battle.GetNode("Units").GetChildren();

        foreach (var unit in units)
        {
            if (unit is BattleUnit bUnit)
                _unitRegistry.RegisterUnit(bUnit, _grid.GetCellAtGlobalPosition(bUnit.GlobalPosition));
        }
        
        _logger.Log($"_registerExistingBattleUnitNodes count={_unitRegistry.Units.Count}", LogSeverity.Info, LogCategory.Initialization);
    }

    private void AbortActivationToFreeSelect()
    {
        _logger.Log("AbortActivationToFreeSelect", LogSeverity.Info, LogCategory.UnitLifecycle);
        // Cancel unit activation, go back to free select.
        if (!TryUndoMove())
            throw new Exception("Try undo move failed, can't reset.");
        
        ExitTargetingMode();
        // TODO - update cursor
        // add enum for different behaviors AbortBehavior { KeepCursor, RecenterOnOrigin }
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
        var isHoveredUnit = _unitRegistry.TryGetUnitAtCell(cell, out var registeredUnit);
        var unit = _selectionController.HoveredUnit;
        Debug.Assert(unit == registeredUnit, "Hovered unit does not match UnitRegistry record for unit at cell.");
        
        _logger.Log($"GenerateMovementPreviewForHoveredCell cell={cell} unit={unit?.UnitName} regUnit={registeredUnit?.UnitName}", LogSeverity.Info, LogCategory.UiNavigation);
        if (unit == null)
            return;
        
        var movementPreview = _moveRangeService.BuildMovementPreview(cell, unit.Movement);
        _unitActivation = new UnitActivationContext(unit, cell);
        _grid.SetMovementPreview(movementPreview);
    }
    
    private void GenerateMovementPreviewForSelectedUnit()
    {
        if (!DebugUtil.Require(IsUnitSelected, "[BattleController.State].GenerateMovementPreviewForSelectedUnit - Unable to generate, no selected unit"))
            return;
        var unit = _selectionController.SelectedUnit;
        if (!DebugUtil.Require(_unitRegistry.TryGetCell(unit, out var cell), "[BattleController.State].GenerateMovementPreviewForSelectedUnit - Unable to generate, no selected unit"))
            return;
        
        _logger.Log($"GenerateMovementPreviewForHoveredCell cell={cell} unit={unit?.UnitName}", LogSeverity.Info, LogCategory.UiNavigation);
        
        var movementPreview = _moveRangeService.BuildMovementPreview(cell, unit!.Movement);
        _unitActivation = new UnitActivationContext(unit, cell);
        _grid.SetMovementPreview(movementPreview);
    }
    
    private void ResetActivationPreview()
    {
        ClearActivationPreviews();
        if (IsUnitSelected)
        {
            GenerateMovementPreviewForSelectedUnit();
            // TODO - center cursor/camera on 
        }
        else
            GenerateMovementPreviewForHoveredCell();
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