#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core.Types;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.Combat.Types;
using Goblinos.Scripts.Core;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Fields */
    private BattleInputState _inputState = BattleInputState.FreeSelect;
    
    private PrimaryActionValidTargetsPreview? _primaryActionPreviews;
    private CombatPreview? _combatPreview;
    
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

    public bool IsPlayerTurn => _turnController.ActiveSide == BattleSide.Player;
    public bool IsUnitSelected => _selectionController.IsUnitSelected;
    public Vector2I HoveredCell => _selectionController.HoveredCell;
    public BattleUnit? HoveredUnit => _selectionController.HoveredUnit;
    public Vector2I? SelectedCell => SelectedUnit != null && _unitRegistry.TryGetCell(SelectedUnit, out var cell) ? cell : null;
    public BattleUnit? SelectedUnit => _selectionController.SelectedUnit;
    
    
    
    private void _Ready_State()
    {
        Debug.Assert(_unitRegistry != null, "[BattleController.Units] Not Initialized. Unable to register UnitRegistry.");
        _registerExistingBattleUnitNodes();
        
        _logger.Log("_Ready_State", GobLogSeverity.Info, GobLogCategory.Initialization);
    }


    public void RequestEndTurn()
    {
        if (!_turnController.RequestEndPlayerTurn())
            return;
        
        AbortActivationToFreeSelect();
    }
    
    private void AbortActivationToFreeSelect()
    {
        _logger.Log("AbortActivationToFreeSelect", GobLogSeverity.Info, GobLogCategory.UnitLifecycle);
        // Cancel unit activation, go back to free select.
        if (InputState == BattleInputState.FreeSelect) return;
        
        if (!TryUndoMove())
            throw new Exception("Try undo move failed, can't reset.");
        
        UnitActivation?.Unit.SetActivationState(UnitActivationState.Ready);
        ClearActivationAndUi();
        EnterFreeSelectMode();
    }

    private void ClearPreviews()
    {
        SetCombatPreview(null);
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
        UnitActivation = null;
    }

    private void DisplayPrimaryActionPreview(PrimaryActionType actionType)
    {
        _logger.Log($"[{nameof(DisplayPrimaryActionPreview)}] actionType={actionType}", GobLogSeverity.Trace, GobLogCategory.Input);
        if (!DebugUtil.Check(_primaryActionPreviews != null,
                $"[{nameof(BattleController)}].{nameof(DisplayPrimaryActionPreview)} - Previews not initialized."))
        {
            _primaryActionPreviews = _primaryActionTargetingService.BuildPrimaryActionValidTargetPreviews(UnitActivation, new[]
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
        _cursor.TriggerUpdateFocus(GridCursorFocusSource.Programmatic);
        GenerateHoverPreview();
        ShowCursor();
    }

    private void EnterMoveTargetingMode(BattleUnit unit)
    {
        _logger.Log("EnterMovementMode", GobLogSeverity.Trace, GobLogCategory.Input);

        if (!DebugUtil.Require(unit != null, "[BattleController.Input] failed to enter MovementMode, no unit"))
        {
            AbortActivationToFreeSelect();
            return;
        }

        if (unit.State == UnitActivationState.Exhausted)
        {
            _logger.Log($"EnterMoveTargetingMode blocked: unit exhausted unit={unit.UnitName}", GobLogSeverity.Trace, GobLogCategory.BattleState);
            AbortActivationToFreeSelect();
            return;
        }
        
        var cell = _selectionController.SelectedCell;
        if (!DebugUtil.Require(cell.HasValue, "[BattleController.Input] failed to enter MovementMode, no selected cell") ||
            !DebugUtil.Require(unit != null, "[BattleController.Input] failed to enter MovementMode, no unit") ||
            !DebugUtil.Require(unit.State != UnitActivationState.Exhausted, $"[BattleController.Input] failed to enter MovementMode, unit={unit.UnitName} already exhausted")
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
        _logger.Log("EnterPrimaryActionSelectMode", GobLogSeverity.Trace, GobLogCategory.Input);
        
        if (!DebugUtil.Require(UnitActivation != null,
                $"[{nameof(BattleController)}].{nameof(EnterPrimaryActionSelectMode)} - No {nameof(UnitActivationContext)}"))
        {
            AbortActivationToFreeSelect();
            return;
        }
            
        
        HideCursor();
        GeneratePrimaryActionTargetPreviewForActiveUnit();
        _hud.ShowPrimaryActionSelectMenu(UnitActivation.Unit, _grid.GetGlobalTopLeftPositionForCell(UnitActivation.DestinationCell) + new Vector2(GlobalSettings.TileSize, 0), _primaryActionPreviews);
        
        InputState = BattleInputState.PrimaryActionSelect;
    }

    private void EnterPrimaryActionTargetMode(PrimaryActionType action)
    {
        _logger.Log("EnterPrimaryActionTargetMode", GobLogSeverity.Info, GobLogCategory.BattleState);
        if (!DebugUtil.Require(UnitActivation != null,
                "Cannot Enter PrimaryActionTarget mode, no UnitActivationContext"))
            return;
        
        // TODO - set cursor position
        ShowCursor();
        InputState = BattleInputState.PrimaryActionTargeting;
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

        if (unit.State == UnitActivationState.Exhausted)
            return;
        
        _logger.Log($"GenerateHoverPreview cell={cell} unit={unit.UnitName} regUnit={registeredUnit?.UnitName}", GobLogSeverity.Extra, GobLogCategory.UiNavigation);
        
        var movementPreview = _moveRangeService.GetMovementPreview(cell, unit);
        var attackPreview = _targetRangeService.BuildThreatUnionFromCells(movementPreview.Cells, unit.AttackRange);
        
        if (unit.IsFriendly)
            _grid.SetUnitStartOfTurnPreviews(movementPreview.Cells, attackPreview);
        else
        {
            var cells = new HashSet<Vector2I>();
            cells.UnionWith(movementPreview.Cells);
            cells.UnionWith(attackPreview);
            _grid.SetHoveredThreatPreview(cells);
        }
    }
    
    private void GenerateMovePreviewForSelectedUnit()
    {
        if (!DebugUtil.Require(IsUnitSelected, "[BattleController.State].GeneratePreviewForSelectedUnit - Unable to generate, no selected unit"))
            return;
        var unit = SelectedUnit;
        if (!DebugUtil.Require(_unitRegistry.TryGetCell(unit, out var cell), "[BattleController.State].GenerateMovementPreviewForSelectedUnit - Unable to generate, no selected unit"))
            return;
        
        _logger.Log($"GeneratePreviewForSelectedUnit cell={cell} unit={unit?.UnitName}", GobLogSeverity.Info, GobLogCategory.UiNavigation);
        
        var movementPreview = _moveRangeService.GetMovementPreview(cell, unit!);
        var attackPreview = _targetRangeService.BuildThreatUnionFromCells(movementPreview.Cells, unit!.AttackRange);

        GD.Print($"movementPreview.Cells.Count={movementPreview.Cells.Count}");
        _grid.SetUnitStartOfTurnPreviews(movementPreview.Cells, attackPreview);
    }

    private void GeneratePrimaryActionTargetPreviewForActiveUnit()
    {
        _logger.Log("EnterPrimaryActionTargetMode", GobLogSeverity.Info, GobLogCategory.BattleState);
        if (!DebugUtil.Require(UnitActivation != null,
                "Unable to generate PrimaryActionTarget preview, no UnitActivationContext"))
            return;

        // TODO - function to get appropriate preview types from Unit.
        _primaryActionPreviews =  _primaryActionTargetingService.BuildPrimaryActionValidTargetPreviews(UnitActivation, new []
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
        unit.SetActivationState(UnitActivationState.Activated);
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

    private void SetCombatPreview(CombatPreview? preview)
    {
        _combatPreview = preview;
        EmitSignal(SignalName.CombatPreviewUpdated, preview); 
    }

    private void UpdateCombatPreview()
    {
        CombatPreview? combatPreview = null;

        if (UnitActivation == null || UnitActivation.PrimaryAction != PrimaryActionType.Attack)
        {
            SetCombatPreview(combatPreview);
            return;
        }
            
        BattleUnit? attacker = UnitActivation.Unit;
        BattleUnit? defender = UnitActivation.PrimaryActionTargetUnit ?? HoveredUnit;

        if (defender == null || defender.IsFriendly)
        {
            SetCombatPreview(combatPreview);
            return;
        }

        var defenderCell = UnitActivation.PrimaryActionTargetCell ?? HoveredCell;
        
        // check if defender is valid target
        if (_primaryActionPreviews == null || !_primaryActionPreviews.IsValidTarget(PrimaryActionType.Attack, defenderCell))
        {
            SetCombatPreview(combatPreview);
            return;
        }
        
        combatPreview = _combatResolver.GetCombatPreview(attacker, defender);
        SetCombatPreview(combatPreview);
    }
    
    /// <summary>
    /// Resolved all pending actions, move + attack/ability/wait
    /// </summary>
    /// This can be refactored into own class - ActionResolver
    /// If many actions or becomes complex, refactor into one class per Action Type
    /// with shared interface containing TryExecute.
    public async void CommitUnitActivation(IUnitActionPlan activation)
    {
        // commit move
        if (activation.HasMove)
            _movementController.CommitMove(activation.Unit, activation.OriginCell, activation.MoveTargetCell!.Value);
        
        // Handle attack / other action. TODO
        switch (activation.PrimaryAction)
        {
            case PrimaryActionType.Attack:
                ResolveCombat(activation);
                break;
            case PrimaryActionType.Ability:
                await ResolveAbility(activation);
                break;
            case PrimaryActionType.Item:
                _logger.Warn($"{nameof(CommitUnitActivation)} - Item not implemented.");
                break;
            case PrimaryActionType.Trade:
                _logger.Warn($"{nameof(CommitUnitActivation)} - Trade not implemented.");
                break;
            case PrimaryActionType.Wait:
                break;
            case PrimaryActionType.None:
                throw new ArgumentOutOfRangeException(nameof(activation), activation, "Unhandled property 'activation.PrimaryAction' - type None");
            default:
                throw new NotImplementedException();
        }

        var unit = activation.Unit;
        
        // Log Snapshot class for logs / after battle stats. TODO
        
        
        if (IsPlayerTurn)
        {
            unit.SetActivationState(UnitActivationState.Exhausted);
        }

        EmitSignal(nameof(UnitActionsResolved), unit);
    }

    private void CommitUnitActivation()
    {
        if (!DebugUtil.Require(UnitActivation != null,
                $"[{nameof(BattleController)}].ResolveUnitActions - No UnitActivationContext"))
        {
            AbortActivationToFreeSelect();
            return;
        }
        CommitUnitActivation(UnitActivation);
    }

    public async Task ResolveAbility(IUnitActionPlan activation)
    {
        _logger.Log("ResolveAbility", GobLogSeverity.Info, GobLogCategory.CombatResolution);
        // TODO - check units are in range
        if (!DebugUtil.Require(activation != null,
                $"[{nameof(BattleController)}].ResolveUnitActions - No UnitActivationContext"))
            return;

        await _abilityResolver.Resolve(activation);
    }

    public async void ResolveCombat(IUnitActionPlan activation)
    {
        // TODO - check units are in range
        if (!DebugUtil.Require(activation != null,
                $"[{nameof(BattleController)}].ResolveUnitActions - No UnitActivationContext"))
            return;
        
        
        
        var results = await _combatResolver.Resolve(activation);
        HandleCombatResults(results);
    }

    public void HandleCombatResults(SimpleCombatResult results)
    {
        if (results.AttackerDied)
            _unitRegistry.DestroyUnit(results.Attacker.UnitId);
        if (results.DefenderDied)
        {
            _unitRegistry.DestroyUnit(results.Defender.UnitId);
            // Level up attacker - TODO - change this to provide exp in all cases
            var attacker = _unitRegistry.GetUnitById(results.Attacker.UnitId);
            var levelUpResults = _context.UnitProgression.LevelUp(attacker.Unit);
            HandleLevelUpResults(levelUpResults);
        }
            
        
        //
    }

    private void HandleLevelUpResults(UnitLeveledUpEvent levelUpResults)
    {
        // var panel = _levelUpResultsPanelScene.Instantiate<LevelUpResultsPanel>();
        // panel.Bind(levelUpResults);
        // _hud.AddChild(panel);
        // panel.Visible = true;

        _hud.DisplayLeveledUpDetails(levelUpResults);
    }

    private void ShowCursor()
    {
        _cursor.TriggerUpdateFocus(GridCursorFocusSource.Programmatic);
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
        
        _movementController.UndoPendingMove(UnitActivation.Unit);

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