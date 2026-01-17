#nullable enable
using System;
using System.Diagnostics;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Fields */
    private BattleInputState _inputState = BattleInputState.FreeSelect;
    

    private PrimaryActionPreviewResults? _primaryActionPreviews;
    
    /** Properties */
    public UnitActivationContext? UnitActivation;
    
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
        
        ClearActivationAndUi();
        EnterFreeSelectMode();
    }

    private void ClearPreviews()
    {
        _grid.ClearOverlays();
    }

    private void ClearActivationAndUi()
    {
        _selectionController.TriggerClearSelection();
        ClearUnitActivation();
        ClearPreviews();
        _hud.HidePrimaryActionSelectMenu();
        _hud.HidePrimaryActionConfirm();
        ShowCursor();
    }
    
    private void ClearUnitActivation()
    {
        // Currently does not account for actions that could prevent undo. Change this if adding traps, reactions etc.
        if (!DebugUtil.Require(UnitActivation != null, "[BattleController.State] ClearUnitActivation failed. No UnitActivationContext."))
            return;
        
        UnitActivation = null;
    }

    private void DisplayPrimaryActionPreview(PrimaryActionType actionType)
    {
        _logger.Log($"[{nameof(DisplayPrimaryActionPreview)}] actionType={actionType}", LogSeverity.Trace, LogCategory.Input);
        if (!DebugUtil.Check(_primaryActionPreviews != null,
                $"[{nameof(BattleController)}].{nameof(DisplayPrimaryActionPreview)} - Previews not initialized."))
        {
            _primaryActionPreviews = _primaryActionTargetingService.BuildPrimaryActionPreviews(UnitActivation, new[]
                {
                    PrimaryActionType.Attack
                });
        }
        
        if (_primaryActionPreviews == null || !_primaryActionPreviews.HasTargets(actionType))
        {
            ClearPreviews();
            return;
        }
        
        var preview = _primaryActionPreviews.GetTargets(actionType);
        _grid.SetActionPreview(preview, actionType);
    }

    private void EnterFreeSelectMode()
    {
        InputState = BattleInputState.FreeSelect;
        GenerateHoverPreview();
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

    private void EnterPrimaryActionConfirmation()
    {
        if (!DebugUtil.Require(UnitActivation != null,
                $"[{nameof(BattleController)}].{nameof(EnterPrimaryActionConfirmation)} - No {nameof(UnitActivationContext)}"))
        {
            AbortActivationToFreeSelect();
            return;
        }
        
        // TODO - display confirmation / battle preview
        _hud.ShowPrimaryActionConfirm();
        InputState = BattleInputState.PrimaryActionConfirm;
    }

    private void EnterPrimaryActionSelectMode()
    {
        _logger.Log("EnterPrimaryActionSelectMode", LogSeverity.Trace, LogCategory.Input);
        
        if (!DebugUtil.Require(UnitActivation != null,
                $"[{nameof(BattleController)}].{nameof(EnterPrimaryActionSelectMode)} - No {nameof(UnitActivationContext)}"))
        {
            AbortActivationToFreeSelect();
            return;
        }
            
        
        HideCursor();
        GeneratePrimaryActionTargetPreviewForActiveUnit();
        _hud.ShowPrimaryActionSelectMenu();
        
        // TODO - create primary action target previews. Add to Activation Context.
        InputState = BattleInputState.PrimaryActionSelect;
    }

    private void EnterPrimaryActionTargetMode(PrimaryActionType action)
    {
        _logger.Log("EnterPrimaryActionTargetMode", LogSeverity.Info, LogCategory.BattleState);
        if (!DebugUtil.Require(UnitActivation != null,
                "Cannot Enter PrimaryActionTarget mode, no UnitActivationContext"))
            return;
        
        UnitActivation.SetPrimaryAction(action);
        ShowCursor();
        _hud.HidePrimaryActionSelectMenu();
        // TODO - set cursor position
        
        InputState = BattleInputState.PrimaryActionTargeting;
    }

    private void ExitPrimaryActionSelectMode()
    {
        _logger.Log("ExitPrimaryActionSelectMode", LogSeverity.Trace, LogCategory.Input);
        
        if (TryUndoMove() && DebugUtil.Require(UnitActivation != null, "[BattleController] ExitPrimaryActionSelectMode - no UnitActivationContext"))
            EnterMoveTargetingMode(UnitActivation.Unit);
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
        
        _logger.Log($"GenerateHoverPreview cell={cell} unit={unit.UnitName} regUnit={registeredUnit?.UnitName}", LogSeverity.Extra, LogCategory.UiNavigation);
        
        var movementPreview = _moveRangeService.GetMovementPreview(cell, unit.Movement);
        var attackPreview = _targetRangeService.BuildThreatUnionFromCells(movementPreview.Cells, unit.AttackRange);
        
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
        var attackPreview = _targetRangeService.BuildThreatUnionFromCells(movementPreview.Cells, unit.AttackRange);

        GD.Print($"movementPreview.Cells.Count={movementPreview.Cells.Count}");
        _grid.SetPreviews(movementPreview, attackPreview);
    }

    private void GeneratePrimaryActionTargetPreviewForActiveUnit()
    {
        _logger.Log("EnterPrimaryActionTargetMode", LogSeverity.Info, LogCategory.BattleState);
        if (!DebugUtil.Require(UnitActivation != null,
                "Unable to generate PrimaryActionTarget preview, no UnitActivationContext"))
            return;

        // TODO - function to get appropriate preview types from Unit.
        _primaryActionPreviews =  _primaryActionTargetingService.BuildPrimaryActionPreviews(UnitActivation, new []
            {
                PrimaryActionType.Attack, 
                PrimaryActionType.Ability
            });
        // TODO - other types of actions.
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

        if (UnitActivation != null && UnitActivation.Unit == unit)
        {
            _logger.Warn("InitializeActivationContext - already initialized.");
            return;
        }
            
        UnitActivation = new UnitActivationContext(unit, cell.Value);
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

        if (!DebugUtil.Require(UnitActivation != null, "[BattleController.State] ResetUnitActivation failed. No UnitActivationContext."))
            return;
        
        UnitActivation.Reset();
    }
    
    /// <summary>
    /// Resolved all pending actions, move + attack/ability/wait
    /// </summary>
    /// This can be refactored into own class - ActionResolver
    /// If many actions or becomes complex, refactor into one class per Action Type
    /// with shared interface containing TryExecute.
    private void ResolveUnitActions()
    {
        // move already committed.
        // Handle attack / other action. TODO
        // Set unit as activated / already taken turn. TODO
        ClearActivationAndUi();
        EnterFreeSelectMode();
    }

    private void ShowCursor()
    {
        _cursor.Visible = true;
    }

    private bool TryUndoMove()
    {
        if (!DebugUtil.Require(UnitActivation != null, "[BattleController.Input].TryUndoMove - Unable to undo move action, no UnitActivationContext"))
            return false;

        if (!UnitActivation.HasMoved)
            return true;
        if (!UnitActivation.CanUndoMove)
            return false;
        if (!_movementController.TryMoveToCell(UnitActivation.Unit, UnitActivation.OriginCell))
            return false;

        UnitActivation.ClearMoveTargetCell();
        _selectionController.UpdateHovered();
        return true;
    }

    private bool TryClearUnitActivation()
    {
        if (DebugUtil.Require(UnitActivation != null, "Unable to undo move action, no UnitActivationContext"))
            return false;
        
        if (!UnitActivation.CanReset)
            return false;

        ClearUnitActivation();
        return true;
    }
}