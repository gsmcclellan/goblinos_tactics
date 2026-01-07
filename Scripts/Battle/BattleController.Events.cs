// DEPRECATED - using direct signals instead of re emiting here. If re emit, use sub node instead of BattleController partial.

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
        _selectionController.SelectedUnitChanged += OnSelectedUnitChanged;
    }

    private void _UnsubscribeFromEvents()
    {
        _selectionController.SelectedUnitChanged -= OnSelectedUnitChanged;
    }
    
    // ---------------------------------------------------------------------
    // Event Methods
    // ---------------------------------------------------------------------

    private void OnSelectedUnitChanged(Node? selectedNode)
    {
        if (selectedNode is BattleUnit bu)
            EnterMoveTargetingMode();
        else if (selectedNode == null)
            ExitTargetingMode();
        else
            throw new Exception("[BattleController.Events] Selected node invalid type.");
    }
    private void NotifyInitialized()
    {
        EmitSignal(SignalName.BattleControllerInitialized);
    }
}