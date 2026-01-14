#nullable enable
using System;
using Goblinos.Logging;
using Goblinos.Scripts.Battle.Types;
using Goblinos.Scripts.Util;
using Godot;

namespace Goblinos.Scripts.Battle;

public partial class BattleController
{
    /** Signals */
    // Battle Level
    [Signal] public delegate void BattleControllerInitializedEventHandler();

    [Signal]
    public delegate void InputStateChangedEventHandler(int state);
    // Components

    /** Events */



    // ---------------------------------------------------------------------
    // Lifecycle / Setup Methods
    // ---------------------------------------------------------------------
    
    private void _Ready_Events()
    {
        // GridCursor
        DebugUtil.Require(_cursor != null, "[GridCursor] not initialized. Unable to set up actions.");
        
        _logger.Log("Ready_Events", LogSeverity.Info, LogCategory.Initialization);
    }

    private void _ExitTree_Actions()
    {
        _UnsubscribeFromEvents();
        _logger.Log("ExitTree_Actions", LogSeverity.Info, LogCategory.Exit);
    }

    private void _SubscribeToEvents()
    {
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
        _logger.Log($"OnHoveredUnitChanged - node={hoveredNode}", LogSeverity.Trace, LogCategory.Signal);
        switch (hoveredNode)
        {
            // In FreeSelect, change move preview when hovered unit changes.
            case BattleUnit when InputState == BattleInputState.FreeSelect:
                ResetPreviews();
                break;
            case null when InputState == BattleInputState.FreeSelect:
                ClearPreviews();
                break;
        }
    }

    private void OnPrimaryActionFocused(int actionIndex)
    {
        var action = (PrimaryActionType)actionIndex;
        _logger.Log($"OnPrimaryActionFocused - action={action}", LogSeverity.Info, LogCategory.Signal);
        
        // TODO - check based on type if requires target - update target preview
    }

    private void OnPrimaryActionSelected(int actionIndex)
    {
        var action = (PrimaryActionType)actionIndex;
        _logger.Log($"OnPrimaryActionSelected - action={action}", LogSeverity.Info, LogCategory.Signal);
        // Transition to next phase based on if it requires target -> targeting phase, else confirm phase
        // TODO - check based on type if requires target.
        EnterPrimaryActionTargetMode(action);
    }
    
    private void OnSelectedUnitChanged(Node? selectedNode)
    {
        _logger.Log($"OnSelectedUnitChanged - node={selectedNode}", LogSeverity.Trace, LogCategory.Signal);
        // removed enter/exit move/action select mode. This now happens in HandleAccept & HandleCancel methods.
    }

    private void OnUnitMoveResolved(BattleUnit unit, Vector2I fromCell, Vector2I toCell)
    {
        _moveRangeService.InvalidateCache();
    }

    private void OnUnitRegistered(BattleUnit unit, Vector2I cell)
    {
        _moveRangeService.InvalidateCache();
    }

    private void OnUnitUnregistered(BattleUnit unit, Vector2I cell)
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