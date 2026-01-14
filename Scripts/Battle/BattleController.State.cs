#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Util;
using Godot;

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

    public Vector2I HoveredCell => _selectionController.HoveredCell;
    public BattleUnit? HoveredUnit => _selectionController.HoveredUnit;
    public Vector2I? SelectedCell => _unitRegistry.TryGetCell(SelectedUnit, out var cell) ? cell : null;
    public BattleUnit? SelectedUnit => _selectionController.SelectedUnit;
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
        ResetPreviews();
    }

    private void ClearPreviews()
    {
        _grid.ClearOverlays();
    }
    private void ClearUnitActivation()
    {
        // Currently does not account for actions that could prevent undo. Change this if adding traps, reactions etc.

        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.State] ClearUnitActivation failed. No UnitActivationContext."))
            return;
        
        _unitActivation = null;
    }
    
    private void EnterMoveTargetingMode(BattleUnit unit)
    {
        _logger.Log("EnterMovementMode", LogSeverity.Trace, LogCategory.Input);
        
        var cell = _selectionController.SelectedCell;
        if (!DebugUtil.Require(cell.HasValue, "[BattleController.Input] failed to enter MovementMode, no selected cell") ||
            !DebugUtil.Require(unit != null, "[BattleController.Input] failed to enter MovementMode, no unit")
           )
            return;
        InitializeActivationContext();
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
        
        HideCursor();
        _hud.ShowPrimaryActionSelectMenu();
        
        // TODO - create primary action target previews. Add to Activation Context.
        InputState = BattleInputState.PrimaryActionSelect;
    }

    private void EnterPrimaryActionTargetMode(PrimaryActionType action)
    {
        if (!DebugUtil.Require(_unitActivation != null,
                "Cannot Enter PrimaryActionTarget mode, no UnitActivationContext"))
            return;
        
        _unitActivation.SetPrimaryAction(action);
        ShowCursor();
        _hud.HidePrimaryActionSelectMenu();
        // TODO - set cursor position
        GeneratePrimaryActionTargetPreviewForActiveUnit();
        InputState = BattleInputState.PrimaryActionTargeting;
    }

    private void ExitPrimaryActionSelectMode()
    {
        _logger.Log("ExitPrimaryActionSelectMode", LogSeverity.Trace, LogCategory.Input);
        
        if (TryUndoMove() && DebugUtil.Require(_unitActivation != null, "[BattleController] ExitPrimaryActionSelectMode - no UnitActivationContext"))
            EnterMoveTargetingMode(_unitActivation.Unit);
    }
        
    private void ExitTargetingMode()
    {
        _logger.Log("ExitTargetingMode", LogSeverity.Trace, LogCategory.Input);
        
        _selectionController.TriggerClearSelection();
        _hud.HidePrimaryActionSelectMenu();
        ClearUnitActivation();
        ShowCursor();
        ResetPreviews();
        
        InputState = BattleInputState.FreeSelect;
    }
    
    /// <summary>
    /// Generates move & attack preview for hovered cell.
    /// </summary>
    private void GenerateHoverPreview()
    {
        var cell = HoveredCell;
        var unit = HoveredUnit;
        if (unit == null)
            return;
        
        _unitRegistry.TryGetUnitAtCell(cell, out var registeredUnit);
        
        if (!DebugUtil.Require(unit == registeredUnit,
                "Hovered unit does not match UnitRegistry record for unit at cell."))
            return;
        
        _logger.Log($"GeneratePreviewForHoveredCell cell={cell} unit={unit.UnitName} regUnit={registeredUnit?.UnitName}", LogSeverity.Info, LogCategory.UiNavigation);
        
        var movementPreview = _moveRangeService.GetMovementPreview(cell, unit.Movement);
        var attackPreview = _attackRangeService.BuildAttackThreatUnionFromCells(movementPreview.Cells, unit.AttackRange);
        
        _grid.SetPreviews(movementPreview, attackPreview);
    }
    
    private void GenerateMovePreviewForSelectedUnit()
    {
        if (!DebugUtil.Require(IsUnitSelected, "[BattleController.State].GeneratePreviewForSelectedUnit - Unable to generate, no selected unit"))
            return;
        var unit = SelectedUnit;
        if (!DebugUtil.Require(_unitRegistry.TryGetCell(unit, out var cell), "[BattleController.State].GenerateMovementPreviewForSelectedUnit - Unable to generate, no selected unit"))
            return;
        
        _logger.Log($"GeneratePreviewForSelectedUnit cell={cell} unit={unit?.UnitName}", LogSeverity.Info, LogCategory.UiNavigation);
        
        var movementPreview = _moveRangeService.GetMovementPreview(cell, unit!.Movement);
        var attackPreview = _attackRangeService.BuildAttackThreatUnionFromCells(movementPreview.Cells, unit.AttackRange);

        GD.Print($"movementPreview.Cells.Count={movementPreview.Cells.Count}");
        _grid.SetPreviews(movementPreview, attackPreview);
    }

    private void GeneratePrimaryActionTargetPreviewForActiveUnit()
    {
        if (!DebugUtil.Require(_unitActivation != null,
                "Unable to generate PrimaryActionTarget preview, no UnitActivationContext"))
            return;
        
        var unit = _unitActivation.Unit;
        var cell = _unitActivation.MoveTargetCell ?? _unitActivation.OriginCell;
        
        // TODO - other types of actions.
        // TODO - filter attack targets cells for valid target units
        var attackableCells = _attackRangeService.BuildAttackRangeFromCell(cell, unit.AttackRange)
            .Where((attackableCell) => _unitRegistry.TryGetUnitAtCell(attackableCell, out var unitAtCell) && !unitAtCell.IsFriendly);
        var attackPreview = new HashSet<Vector2I>(attackableCells);
        
        _grid.ClearPreviews();
        _grid.SetAttackPreview(attackPreview);
    }

    private void HideCursor()
    {
        _cursor.Visible = false;
    }

    private void InitializeActivationContext()
    {
        var unit = SelectedUnit;
        var cell = SelectedCell;
        if (!DebugUtil.Require(unit != null, "[BattleController] InitializeActivationContext - No Unit") ||
            !DebugUtil.Require(cell != null, "[BattleController] InitializeActivationContext - No Cell"))
            return;

        if (_unitActivation != null && _unitActivation.Unit == unit)
        {
            _logger.Warn("InitializeActivationContext - already initialized.");
            return;
        }
            
        _unitActivation = new UnitActivationContext(unit, cell.Value);
    }
    
    private void ResetPreviews()
    {
        ClearPreviews();
        
        if (IsUnitSelected)
        {
            GenerateMovePreviewForSelectedUnit();
            return;
        }

        GenerateHoverPreview();
    }
    
    private void ResetUnitActivation()
    {
        // Currently does not account for actions that could prevent undo. Change this if adding traps, reactions etc.

        if (!DebugUtil.Require(_unitActivation != null, "[BattleController.State] ResetUnitActivation failed. No UnitActivationContext."))
            return;
        
        _unitActivation.Reset();
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

    private void ShowCursor()
    {
        _cursor.Visible = true;
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