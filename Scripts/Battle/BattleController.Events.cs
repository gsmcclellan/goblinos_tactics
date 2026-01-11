using System;
using System.Diagnostics;
using Goblinos.Logging;
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
        _selectionController.HoveredUnitChanged += OnHoveredUnitChanged;
        _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;
    }

    private void _UnsubscribeFromEvents()
    {
        _selectionController.HoveredUnitChanged -= OnHoveredUnitChanged;
        _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
    }
    
    // ---------------------------------------------------------------------
    // Event Methods
    // ---------------------------------------------------------------------

    // TODO - on Hovered unit change, show move & attack preview
    private void OnHoveredUnitChanged(Node? hoveredNode)
    {
        // In FreeSelect, change move preview when hovered unit changes.
        
        
        if (hoveredNode is BattleUnit && InputState == BattleInputState.FreeSelect)
            ResetActivationPreview();
        else if (hoveredNode == null && InputState == BattleInputState.FreeSelect)
            ClearActivationPreviews();
    }
    private void OnSelectedUnitChanged(Node? selectedNode)
    {
        if (selectedNode is BattleUnit bu)
            EnterMoveTargetingMode(bu);
        else if (selectedNode == null)
            ExitTargetingMode();
        else
            throw new Exception("[BattleController.Events] Selected node invalid type.");
    }
    private void NotifyInitialized()
    {
        EmitSignal(SignalName.BattleControllerInitialized);
    }

    private void NotifyInputStateChanged(BattleInputState state)
    {
        EmitSignal(SignalName.InputStateChanged, (int)state);
    }
}