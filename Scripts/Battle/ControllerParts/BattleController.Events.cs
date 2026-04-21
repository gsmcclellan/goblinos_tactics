#nullable enable
using System;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Core.Types;
using Goblinos.Scripts.Battle.Preview;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Battle.Units;
using Goblinos.Scripts.UI.Battle;
using Goblinos.Scripts.Units;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Signals */
    // Battle Level
    [Signal] 
    public delegate void BattleControllerInitializedEventHandler();
    [Signal]
    public delegate void InputStateChangedEventHandler(int state);
    // Units
    [Signal]
    public delegate void UnitActionsResolvedEventHandler(BattleUnit unit); // TODO - put unit context snapshot here.
    [Signal]
    public delegate void CombatPreviewUpdatedEventHandler(CombatPreview? combatPreview);

    /** Events */



    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    private void _Ready_Events()
    {
        // GridCursor
        DebugUtil.Require(_cursor != null, "[GridCursor] not initialized. Unable to set up actions.");
        _logger.Log("Ready_Events", GobLogSeverity.Info, GobLogCategory.Initialization);
    }

    private void _ExitTree_Actions()
    {
        _UnsubscribeFromEvents();
        _logger.Log("ExitTree_Actions", GobLogSeverity.Info, GobLogCategory.Exit);
    }

    private void _SubscribeToEvents()
    {
        // BattleController
        UnitActionsResolved += OnUnitActionsResolved;
        
        // BattleHud
        _hud.PrimaryActionFocused += OnPrimaryActionFocused;
        _hud.PrimaryActionSelected += OnPrimaryActionSelected;
        
        // SelectionController
        _selectionController.HoveredUnitChanged += OnHoveredUnitChanged;
        _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;
        
        // UnitRegistry
        _unitRegistry.UnitMoveResolved += OnUnitMoveResolved;
        _unitRegistry.UnitRegistered += OnUnitRegistered;
        _unitRegistry.UnitUnregistered += OnUnitUnregistered;
    }

    private void _UnsubscribeFromEvents()
    {
        // BattleController
        UnitActionsResolved -= OnUnitActionsResolved;
        
        // BattleHud
        _hud.PrimaryActionFocused -= OnPrimaryActionFocused;
        _hud.PrimaryActionSelected -= OnPrimaryActionSelected;
        
        // SelectionController
        _selectionController.HoveredUnitChanged -= OnHoveredUnitChanged;
        _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
        
        // UnitRegistry
        _unitRegistry.UnitMoveResolved -= OnUnitMoveResolved;
        _unitRegistry.UnitRegistered -= OnUnitRegistered;
        _unitRegistry.UnitUnregistered -= OnUnitUnregistered;
    }
    
    // ---------------------------------------------------------------------
    // Event Handlers
    // ---------------------------------------------------------------------

    private void OnHoveredUnitChanged(Node? hoveredNode)
    {
        _logger.Log($"OnHoveredUnitChanged - node={hoveredNode}", GobLogSeverity.Trace, GobLogCategory.Signal);
        switch (hoveredNode)
        {
            // In FreeSelect, change move preview when hovered unit changes.
            case BattleUnit when InputState == BattleInputState.FreeSelect:
                ResetPreviews();
                break;
            // In PrimaryActionTargeting, update combat preview
            case BattleUnit when InputState == BattleInputState.PrimaryActionTargeting:
                UpdateCombatPreview();
                break;
            case null when InputState == BattleInputState.FreeSelect:
                ClearPreviews();
                break;
            case null when InputState == BattleInputState.PrimaryActionTargeting:
                SetCombatPreview(null);
                break;
        }
    }

    private void OnPrimaryActionFocused(int actionIndex)
    {
        // When menu item for primary action is hovered or tabbed / navigated to with buttons
        var action = (PrimaryActionType)actionIndex;
        _logger.Log($"OnPrimaryActionFocused - action={action}", GobLogSeverity.Info, GobLogCategory.Signal);
        DisplayPrimaryActionPreview((PrimaryActionType)actionIndex);
    }

    private void OnPrimaryActionSelected(int actionIndex)
    {
        var action = (PrimaryActionType)actionIndex;
        _logger.Log($"OnPrimaryActionSelected - action={action}", GobLogSeverity.Info, GobLogCategory.Signal);
        if (!DebugUtil.Require(UnitActivation != null,
                "Unable to process selected primary action, null UnitActivationContext"))
            return;
        
        // Transition to next phase based on if it requires target -> targeting phase, else confirm phase
        // TODO - check based on type if requires target.
        
        UnitActivation.SetPrimaryAction(action);
        
        _hud.HidePrimaryActionSelectMenu();
        
        switch (action)
        {
            case PrimaryActionType.Attack:
                EnterPrimaryActionTargetMode(action);
                break;
            case PrimaryActionType.Ability:
                EnterPrimaryActionTargetMode(action);
                break;
            case PrimaryActionType.Item:
                _logger.Warn("Item primary action not implemented");
                break;
            case PrimaryActionType.Trade:
                _logger.Warn("Trade primary action not implemented");
                break;
            case PrimaryActionType.Wait:
                EnterPrimaryActionConfirmation();
                break;
            case PrimaryActionType.None:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void OnSelectedUnitChanged(Node? selectedNode)
    {
        _logger.Log($"OnSelectedUnitChanged - node={selectedNode}", GobLogSeverity.Trace, GobLogCategory.Signal);
        ResetPreviews();
        // removed enter/exit move/action select mode. This now happens in HandleAccept & HandleCancel methods.
    }

    private void OnUnitActionsResolved(BattleUnit unit)
    {
        _moveRangeService.InvalidateCache();
        _turnController.HandleUnitExhausted(unit);
        ClearActivationAndUi();
        EnterFreeSelectMode();
        AbortActivationToFreeSelect();
    }

    private void OnUnitMoveResolved(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        _moveRangeService.InvalidateCache();
        _selectionController.UpdateHovered();
    }

    private void OnUnitRegistered(BattleUnit unit, Vector2I cell)
    {
        _moveRangeService.InvalidateCache();
    }

    private void OnUnitUnregistered(BattleUnit unit, Vector2I cell, bool isUnitDeath)
    {
        _moveRangeService.InvalidateCache();
    }
    
    // ---------------------------------------------------------------------
    // Event Triggers
    // ---------------------------------------------------------------------
    
    private void NotifyInitialized()
    {
        EmitSignal(SignalName.BattleControllerInitialized);
    }
    
    private void NotifyInputStateChanged(BattleInputState state)
    {
        // TODO - hide / show cursor (maybe do somewhere else, possibly on cursor with event handler).
        EmitSignal(SignalName.InputStateChanged, (int)state);
    }
}